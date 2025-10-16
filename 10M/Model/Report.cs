using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MovieLensApp.Model
{
    public class ReportGenerator
    {
        private readonly List<Movie> movies;
        private readonly List<Rating> ratings;
        private const int CHUNK_SIZE = 100000; // Adjust based on your machine

        public ReportGenerator(List<Movie> movies, List<Rating> ratings)
        {
            this.movies = movies;
            this.ratings = ratings;
        }

        public void GenerateReports(bool multithreaded, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            if (multithreaded)
                GenerateWithThreads(outputDir);
            else
                GenerateWithoutThreads(outputDir);
        }

        // ---------- SINGLE-THREADED (OPTIMIZED) ----------
        private void GenerateWithoutThreads(string outputDir)
        {
            GenerateTop10Movies(outputDir, ratings);
            GenerateTop10MoviesByGenre(outputDir, "Action", ratings);
            GenerateTop10MoviesByGenre(outputDir, "Drama", ratings);
            GenerateTop10MoviesByGenre(outputDir, "Comedy", ratings);
            GenerateTop10MoviesByGenre(outputDir, "Fantasy", ratings);
        }

        // ---------- MULTI-THREADED WITH CHUNKING ----------
        private void GenerateWithThreads(string outputDir)
        {
            var chunks = SplitIntoChunks(ratings, CHUNK_SIZE);
            var lockObj = new object();

            var top10AllMovies = new List<(int MovieId, double Avg)>();
            var genreResults = new Dictionary<string, List<(int MovieId, double Avg)>>()
            {
                ["Action"] = new(),
                ["Drama"] = new(),
                ["Comedy"] = new(),
                ["Fantasy"] = new()
            };

            Parallel.ForEach(chunks, chunk =>
            {
                var localTop = chunk
                    .GroupBy(r => r.MovieId)
                    .Select(g => (MovieId: g.Key, Avg: g.Average(x => x.RatingValue)))
                    .ToList();

                var localGenreResults = new Dictionary<string, List<(int MovieId, double Avg)>>();

                foreach (var genre in genreResults.Keys)
                {
                    var genreMovies = movies
                        .Where(m => m.Genres.Contains(genre, StringComparison.OrdinalIgnoreCase))
                        .Select(m => m.MovieId)
                        .ToHashSet();

                    var localGenre = localTop
                        .Where(x => genreMovies.Contains(x.MovieId))
                        .ToList();

                    localGenreResults[genre] = localGenre;
                }

                lock (lockObj)
                {
                    top10AllMovies.AddRange(localTop);
                    foreach (var genre in genreResults.Keys)
                        genreResults[genre].AddRange(localGenreResults[genre]);
                }
            });

            var mergedAllMovies = MergeAverages(top10AllMovies).Take(10).ToList();
            WriteFile(Path.Combine(outputDir, "Top10Movies.txt"), "Top 10 Movies (General)",
                      mergedAllMovies.Select(x => $"{GetMovieTitle(x.MovieId)} :: {x.Avg:F2}"));

            foreach (var genre in genreResults.Keys)
            {
                var mergedGenre = MergeAverages(genreResults[genre]).Take(10).ToList();
                WriteFile(Path.Combine(outputDir, $"Top10_{genre}_Movies.txt"),
                          $"Top 10 {genre} Movies",
                          mergedGenre.Select(x => $"{GetMovieTitle(x.MovieId)} :: {x.Avg:F2}"));
            }
        }

        // ---------- HELPERS ----------

        private static List<List<Rating>> SplitIntoChunks(List<Rating> src, int size)
        {
            var list = new List<List<Rating>>();
            for (int i = 0; i < src.Count; i += size)
                list.Add(src.Skip(i).Take(Math.Min(size, src.Count - i)).ToList());
            return list;
        }

        private static IEnumerable<(int MovieId, double Avg)> MergeAverages(IEnumerable<(int MovieId, double Avg)> results)
        {
            return results
                .GroupBy(x => x.MovieId)
                .Select(g => (MovieId: g.Key, Avg: g.Average(x => x.Avg)))
                .OrderByDescending(x => x.Avg);
        }

        private void GenerateTop10Movies(string outputDir, List<Rating> data)
        {
            var top10 = data
                .GroupBy(r => r.MovieId)
                .Select(g => new { MovieId = g.Key, Avg = g.Average(x => x.RatingValue) })
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();

            WriteFile(Path.Combine(outputDir, "Top10Movies.txt"),
                      "Top 10 Movies (General)",
                      top10.Select(x => $"{GetMovieTitle(x.MovieId)} :: {x.Avg:F2}"));
        }

        private void GenerateTop10MoviesByGenre(string outputDir, string genre, List<Rating> data)
        {
            // ? PRECOMPUTE GENRE MOVIE IDS INTO HASHSET
            var genreMovieIds = movies
                .Where(m => m.Genres.Contains(genre, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.MovieId)
                .ToHashSet();

            // ? FAST FILTER USING HASHSET (O(1) lookup)
            var top10 = data
                .Where(r => genreMovieIds.Contains(r.MovieId))
                .GroupBy(r => r.MovieId)
                .Select(g => new { MovieId = g.Key, Avg = g.Average(r => r.RatingValue) })
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();

            WriteFile(Path.Combine(outputDir, $"Top10_{genre}_Movies.txt"),
                      $"Top 10 {genre} Movies",
                      top10.Select(x => $"{GetMovieTitle(x.MovieId)} :: {x.Avg:F2}"));
        }

        private string GetMovieTitle(int movieId)
            => movies.FirstOrDefault(m => m.MovieId == movieId)?.Title ?? "Unknown";

        private void WriteFile(string path, string header, IEnumerable<string> lines)
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine(header);
            writer.WriteLine("==========================");
            foreach (var line in lines)
                writer.WriteLine(line);
        }
    }
}
