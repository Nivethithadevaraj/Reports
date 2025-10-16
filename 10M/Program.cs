using MovieLensApp.Controller;
using MovieLensApp.View;

namespace MovieLensApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var view = new ConsoleView();
            var controller = new AnalyticsController(view);

            view.ShowMessage("==================== MovieLens Analytics ====================");
            controller.Run();
        }
    }
}
