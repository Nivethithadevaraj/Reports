using System;
using Reports.Controller;

namespace Reports.View
{
    public static class ReportView
    {
        public static void Run()
        {
            string dataPath = @"Data\100k\ml-100k";
            string outputPath = @"output";
            var controller = new ReportController(dataPath, outputPath);

            while (true)
            {
                Console.WriteLine("\n=== MovieLens OLAP Report Generator ===");
                Console.WriteLine("1. Generate Reports (With Multithreading)");
                Console.WriteLine("2. Generate Reports (Without Multithreading)");
                Console.WriteLine("3. Exit");
                Console.Write("Enter choice: ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        controller.GenerateReports(true);
                        break;
                    case "2":
                        controller.GenerateReports(false);
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
        }
    }
}