using MovieLensApp.Controller;
using MovieLensApp.View;

namespace MovieLensApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var view = new ConsoleView();
            var controller = new AnalyticsController(view);
            controller.Run();
        }
    }
}
