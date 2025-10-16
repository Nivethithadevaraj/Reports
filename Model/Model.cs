using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reports.Model
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<string> Genres { get; set; } = new List<string>();
    }

    public class User
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = "";
    }

    public class Rating
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public double Score { get; set; }
    }

    public static class MovieLensDataLoader
    {
        // Reads u.item
        public static Dictionary<int, Movie> LoadMovies(string path)
        {
            var movies = new Dictionary<int, Movie>();
            string[] genreNames = {
                "Unknown","Action","Adventure","Animation","Children's","Comedy",
                "Crime","Documentary","Drama","Fantasy","Film-Noir","Horror",
                "Musical","Mystery","Romance","Sci-Fi","Thriller","War","Western"
            };

            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split('|');
                int movieId = int.Parse(parts[0]);
                string title = parts[1];
                var genres = new List<string>();

                for (int i = 0; i < 19; i++)
                {
                    if (parts[5 + i] == "1") genres.Add(genreNames[i]);
                }

                movies[movieId] = new Movie { Id = movieId, Title = title, Genres = genres };
            }
            return movies;
        }

        // Reads u.user
        public static Dictionary<int, User> LoadUsers(string path)
        {
            var users = new Dictionary<int, User>();
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split('|');
                int userId = int.Parse(parts[0]);
                int age = int.Parse(parts[1]);
                string gender = parts[2];
                users[userId] = new User { Id = userId, Age = age, Gender = gender };
            }
            return users;
        }

        // Streams u.data
        public static IEnumerable<Rating> StreamRatings(string path)
        {
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split('\t');
                yield return new Rating
                {
                    UserId = int.Parse(parts[0]),
                    MovieId = int.Parse(parts[1]),
                    Score = double.Parse(parts[2])
                };
            }
        }
    }
}
