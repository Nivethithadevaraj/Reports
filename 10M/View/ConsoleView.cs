using System;

namespace MovieLensApp.View
{
    public class ConsoleView
    {
        public int ShowMenu()
        {
            Console.WriteLine("==================== MovieLens Analytics ====================");
            Console.WriteLine("1. Generate Reports (Without Threads)");
            Console.WriteLine("2. Generate Reports (With Threads)");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice: ");
            if (int.TryParse(Console.ReadLine(), out int choice))
                return choice;
            return -1;
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
