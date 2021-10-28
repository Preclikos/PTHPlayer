using PTHPlayer.HTSP;
using System;
using System.Threading;

namespace ConnectionTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            var listener = new HTSListener();
            var connection = new HTSConnectionAsync(listener, "C# Test", "0.0.0");

            connection.ErrorHandler += Connection_ErrorHandler;

            /*
            if (!connection.Open("192.168.1.20", 9982))
            {
                Console.WriteLine("Connection Error");
            }

            if (!connection.Authenticate("preclikos", "centr", CancellationToken.None))
            {
                Console.WriteLine("Login Error");
            }

            connection.Stop();*/

            Console.WriteLine("Test Done");

            connection.Start("192.168.1.210", 9982, "preclikos", "centrum");
            Console.ReadKey();

            connection.Stop(true);

            Console.ReadKey();
        }

        private static void Connection_ErrorHandler(object sender, HTSPErrorArgs e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}
