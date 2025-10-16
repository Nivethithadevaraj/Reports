using System;
using System.Diagnostics;
using System.IO;
using MovieLensApp.Model;
using MovieLensApp.View;

namespace MovieLensApp.Controller
{
	public class AnalyticsController
	{
		private readonly ConsoleView view;

		public AnalyticsController(ConsoleView view)
		{
			this.view = view;
		}

		public void Run()
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));
			string moviesPath = Path.Combine(projectRoot, "Data", "movies.dat");
			string ratingsPath = Path.Combine(projectRoot, "Data", "ratings.dat");


			view.ShowMessage("Loading data...");
			var movies = DataLoader.LoadMovies(moviesPath);
			var ratings = DataLoader.LoadRatings(ratingsPath);

			var generator = new ReportGenerator(movies, ratings);

			bool exit = false;
			while (!exit)
			{
				int choice = view.ShowMenu();

				switch (choice)
				{
					case 1:
						view.ShowMessage("Generating reports WITHOUT threads...");
						RunAndMeasure(() => generator.GenerateReports(false, @"output\WithoutThreads"), "Without Threads");
						break;

					case 2:
						view.ShowMessage("Generating reports WITH threads...");
						RunAndMeasure(() => generator.GenerateReports(true, @"output\WithThreads"), "With Threads");
						break;

					case 3:
						view.ShowMessage("Exiting application...");
						exit = true;
						break;

					default:
						view.ShowMessage("Invalid choice. Try again.");
						break;
				}
			}
		}

		private void RunAndMeasure(Action action, string label)
		{
			var sw = Stopwatch.StartNew();
			action();
			sw.Stop();
			view.ShowMessage($"Execution Time ({label}): {sw.Elapsed.TotalSeconds:F2} seconds\n");
		}
	}
}
