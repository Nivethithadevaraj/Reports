using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MovieLensApp.Model
{
    public class ReportGenerator
    {
        private readonly List<Movie> _movies;
        private readonly List<Rating> _ratings;
        private const int CHUNK_SIZE = 100000; // adjust based on dataset size

        public ReportGenerator(List<Movie> movies, List<Rating> ratings)
        {
            _movies = movies;
            _ratings = ratings;
        }

        public void GenerateReports(bool multithreaded, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (multithreaded)
                GenerateWithThreads(outputDir);
            else
                GenerateWithoutThreads(outputDir);
        }

        // ---------------- Without Threads ----------------
        private void GenerateWithoutThreads(string outputDir)
        {
            var movieRatings = GroupRatings(_ratings);
            GenerateReportsCommon(movieRatings, outputDir);
        }

        // ---------------- With Threads ----------------
        private void GenerateWithThreads(string outputDir)
        {
            var chunks = SplitIntoChunks(_ratings, CHUNK_SIZE);
            var combined = new Dictionary<int, List<double>>();
            var lockObj = new object();

            Parallel.ForEach(chunks, chunk =>
            {
                var local = GroupRatings(chunk);
                lock (lockObj)
                {
                    foreach (var kvp in local)
                    {
                        if (!combined.ContainsKey(kvp.Key))
                            combined[kvp.Key] = new List<double>();
                        combined[kvp.Key].AddRange(kvp.Value);
                    }
                }
            });

            GenerateReportsCommon(combined, outputDir);
        }

        // ---------------- Common Logic ----------------
        private void GenerateReportsCommon(Dictionary<int, List<double>> groupedRatings, string outputDir)
        {
            var top10 = groupedRatings
                .Select(g => new { MovieId = g.Key, Avg = g.Value.Average() })
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();

            WriteFile(Path.Combine(outputDir, "Top10Movies.txt"), "Top 10 Movies", top10);

            GenerateTop10ByGenre(outputDir, "Action", groupedRatings);
            GenerateTop10ByGenre(outputDir, "Drama", groupedRatings);
            GenerateTop10ByGenre(outputDir, "Comedy", groupedRatings);
            GenerateTop10ByGenre(outputDir, "Fantasy", groupedRatings);
        }

        private void GenerateTop10ByGenre(string outputDir, string genre, Dictionary<int, List<double>> groupedRatings)
        {
            var genreMovies = new HashSet<int>(
                _movies.Where(m => m.Genres.Contains(genre, StringComparison.OrdinalIgnoreCase))
                       .Select(m => m.MovieId));

            var top10 = groupedRatings
                .Where(g => genreMovies.Contains(g.Key))
                .Select(g => new { MovieId = g.Key, Avg = g.Value.Average() })
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();

            WriteFile(Path.Combine(outputDir, $"Top10_{genre}_Movies.txt"), $"Top 10 {genre} Movies", top10);
        }

        // ---------------- Helpers ----------------
        private Dictionary<int, List<double>> GroupRatings(List<Rating> ratings)
        {
            var dict = new Dictionary<int, List<double>>();
            foreach (var r in ratings)
            {
                if (!dict.TryGetValue(r.MovieId, out var list))
                {
                    list = new List<double>();
                    dict[r.MovieId] = list;
                }
                list.Add(r.RatingValue);
            }
            return dict;
        }

        private static List<List<Rating>> SplitIntoChunks(List<Rating> src, int size)
        {
            var chunks = new List<List<Rating>>();
            for (int i = 0; i < src.Count; i += size)
                chunks.Add(src.Skip(i).Take(Math.Min(size, src.Count - i)).ToList());
            return chunks;
        }

        private string GetMovieTitle(int id)
        {
            return _movies.FirstOrDefault(m => m.MovieId == id)?.Title ?? "Unknown";
        }

        private void WriteFile(string path, string header, IEnumerable<dynamic> entries)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine(header);
            writer.WriteLine("==========================");
            foreach (var e in entries)
                writer.WriteLine($"{GetMovieTitle(e.MovieId)} :: {e.Avg:F2}");
        }
    }
}
