namespace MovieLensApp.Model
{
	public class Movie
	{
		public int MovieId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Genres { get; set; } = string.Empty;
	}
}