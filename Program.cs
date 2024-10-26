using System;

namespace TicTacToeTCP
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter 'server' to start a server or 'client' to connect as a client:");
            string input = Console.ReadLine();

            if (input.ToLower() == "server")
            {
                Server server = new Server();
                server.Start();
            }
            else if (input.ToLower() == "client")
            {
                Client client = new Client();
                client.Start();
            }
            else
            {
                Console.WriteLine("Invalid input. Please restart and enter 'server' or 'client'.");
            }
        }
    }
}
