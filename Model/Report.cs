namespace MovieLens100K.Model
{
    public class Report
    {
        public string ReportName { get; set; } = "";
        public double ExecutionTime { get; set; }
        public bool UsedMultithreading { get; set; }
    }
}
