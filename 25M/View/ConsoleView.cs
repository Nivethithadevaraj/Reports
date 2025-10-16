using System;

namespace MovieLensApp.View
{
    public class ConsoleView
    {
        public int ShowMenu()
        {
            Console.WriteLine("\n===== MovieLens Analytics =====");
            Console.WriteLine("1. Generate Reports (Without Threads)");
            Console.WriteLine("2. Generate Reports (With Threads)");
            Console.WriteLine("3. Exit");
            Console.Write("Enter choice: ");

            return int.TryParse(Console.ReadLine(), out int choice) ? choice : -1;
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
