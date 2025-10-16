using System;
using System.Collections.Generic;
using System.IO;

namespace MovieLensApp.Model
{
    public static class DataLoader
    {
        public static List<Movie> LoadMovies(string path)
        {
            var movies = new List<Movie>();
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split(',', 3); // id,title,genres
                if (parts.Length == 3 && int.TryParse(parts[0], out int id))
                {
                    movies.Add(new Movie
                    {
                        MovieId = id,
                        Title = parts[1],
                        Genres = parts[2]
                    });
                }
            }
            return movies;
        }

        public static List<Rating> LoadRatings(string path)
        {
            var ratings = new List<Rating>();
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split(',');
                if (parts.Length >= 3 &&
                    int.TryParse(parts[0], out int userId) &&
                    int.TryParse(parts[1], out int movieId) &&
                    double.TryParse(parts[2], out double rating))
                {
                    ratings.Add(new Rating
                    {
                        UserId = userId,
                        MovieId = movieId,
                        RatingValue = rating
                    });
                }
            }
            return ratings;
        }
    }
}
