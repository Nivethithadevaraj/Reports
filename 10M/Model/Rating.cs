namespace MovieLensApp.Model
{
    public class Rating
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public double RatingValue { get; set; }
    }
}
