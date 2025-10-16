using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MovieLensApp.Model
{
    public static class DataLoader
    {
        public static List<Movie> LoadMovies(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Movies file not found: {filePath}");

            return File.ReadLines(filePath)
                .Select(line =>
                {
                    var parts = line.Split("::");
                    return new Movie
                    {
                        MovieId = int.Parse(parts[0]),
                        Title = parts[1],
                        Genres = parts[2]
                    };
                })
                .ToList();
        }

        public static List<Rating> LoadRatings(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Ratings file not found: {filePath}");

            return File.ReadLines(filePath)
                .Select(line =>
                {
                    var parts = line.Split("::");
                    return new Rating
                    {
                        UserId = int.Parse(parts[0]),
                        MovieId = int.Parse(parts[1]),
                        RatingValue = double.Parse(parts[2])
                    };
                })
                .ToList();
        }
    }
}
