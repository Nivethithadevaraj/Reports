using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Reports.Model;

namespace Reports.Controller
{
    public class ReportController
    {
        private readonly string _dataPath;
        private readonly string _outputPath;
        private readonly Dictionary<int, Movie> _movies;
        private readonly Dictionary<int, User> _users;

        public ReportController(string dataPath, string outputPath)
        {
            _dataPath = dataPath;
            _outputPath = outputPath;
            _movies = MovieLensDataLoader.LoadMovies(Path.Combine(dataPath, "u.item"));
            _users = MovieLensDataLoader.LoadUsers(Path.Combine(dataPath, "u.user"));
        }

        // --- Main entry point for reports ---
        public void GenerateReports(bool useMultithreading)
        {
            string folder = Path.Combine(_outputPath,
                useMultithreading ? "withmultithreading" : "withoutmultithreading");
            Directory.CreateDirectory(folder);

            var start = DateTime.Now;

            if (useMultithreading)
                GenerateReportsWithThreads(folder);
            else
                GenerateReportsWithoutThreads(folder);

            var elapsed = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine("\n=== Report Summary ===");
            Console.WriteLine("Report Name       : MovieLens Analytics Report");
            Console.WriteLine($"Used Multithreads : {useMultithreading}");
            Console.WriteLine($"Execution Time    : {elapsed:F2} seconds");
            Console.WriteLine("=======================\n");
        }

        // --- Report generation (no threads) ---
        private void GenerateReportsWithoutThreads(string folder)
        {
            var ratings = MovieLensDataLoader.StreamRatings(Path.Combine(_dataPath, "u.data")).ToList();
            GenerateAllReports(ratings, folder);
        }

        // --- Report generation (multi-threaded) ---
        private void GenerateReportsWithThreads(string folder)
        {
            string file = Path.Combine(_dataPath, "u.data");
            int chunkSize = 10000;
            int totalLines = File.ReadLines(file).Count();
            int threadCount = (int)Math.Ceiling(totalLines / (double)chunkSize);

            var chunks = new List<List<Rating>>();
            for (int i = 0; i < threadCount; i++) chunks.Add(new List<Rating>());

            int currentLine = 0;
            foreach (var rating in MovieLensDataLoader.StreamRatings(file))
            {
                int chunkIndex = currentLine / chunkSize;
                chunks[chunkIndex].Add(rating);
                currentLine++;
            }

            var combined = new List<Rating>();
            var threads = new List<Thread>();

            foreach (var chunk in chunks)
            {
                var t = new Thread(() =>
                {
                    lock (combined)
                    {
                        combined.AddRange(chunk);
                    }
                });
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads) t.Join();

            GenerateAllReports(combined, folder);
        }

        // --- Shared report logic ---
        private void GenerateAllReports(List<Rating> ratings, string folder)
        {
            WriteReport(folder, "Top10General.csv", GetTopMovies(ratings, 10));
            WriteReport(folder, "Top10Male.csv", GetTopMoviesByGender(ratings, "M", 10));
            WriteReport(folder, "Top10Female.csv", GetTopMoviesByGender(ratings, "F", 10));

            WriteReport(folder, "Top10Action.csv", GetTopMoviesByGenre(ratings, "Action", 10));
            WriteReport(folder, "Top10Drama.csv", GetTopMoviesByGenre(ratings, "Drama", 10));
            WriteReport(folder, "Top10Comedy.csv", GetTopMoviesByGenre(ratings, "Comedy", 10));
            WriteReport(folder, "Top10Fantasy.csv", GetTopMoviesByGenre(ratings, "Fantasy", 10));

            WriteReport(folder, "Top10AgeBelow18.csv", GetTopMoviesByAge(ratings, 0, 18, 10));
            WriteReport(folder, "Top10Age18to30.csv", GetTopMoviesByAge(ratings, 18, 30, 10));
            WriteReport(folder, "Top10AgeAbove30.csv", GetTopMoviesByAge(ratings, 30, int.MaxValue, 10));
        }

        // --- Report filters ---
        private List<(string Title, double Avg, int Count)> GetTopMovies(List<Rating> ratings, int top)
        {
            return ratings.GroupBy(r => r.MovieId)
                          .Select(g => (Title: _movies[g.Key].Title,
                                        Avg: g.Average(r => r.Score),
                                        Count: g.Count()))
                          .OrderByDescending(x => x.Avg)
                          .ThenByDescending(x => x.Count)
                          .Take(top)
                          .ToList();
        }

        private List<(string Title, double Avg, int Count)> GetTopMoviesByGender(List<Rating> ratings, string gender, int top)
        {
            var filtered = ratings.Where(r => _users.ContainsKey(r.UserId) && _users[r.UserId].Gender == gender);
            return GetTopMovies(filtered.ToList(), top);
        }

        private List<(string Title, double Avg, int Count)> GetTopMoviesByGenre(List<Rating> ratings, string genre, int top)
        {
            var filtered = ratings.Where(r => _movies.ContainsKey(r.MovieId) &&
                                              _movies[r.MovieId].Genres.Contains(genre));
            return GetTopMovies(filtered.ToList(), top);
        }

        private List<(string Title, double Avg, int Count)> GetTopMoviesByAge(List<Rating> ratings, int min, int max, int top)
        {
            var filtered = ratings.Where(r => _users.ContainsKey(r.UserId))
                                  .Where(r =>
                                  {
                                      int age = _users[r.UserId].Age;
                                      return age >= min && age < max;
                                  });
            return GetTopMovies(filtered.ToList(), top);
        }

        // --- CSV writer ---
        private void WriteReport(string folder, string fileName, List<(string Title, double Avg, int Count)> data)
        {
            string filePath = Path.Combine(folder, fileName);
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Movie,AverageRating,RatingCount");
                foreach (var item in data)
                {
                    writer.WriteLine($"\"{item.Title}\",{item.Avg:F2},{item.Count}");
                }
            }
        }
    }
}