using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeTCP
{
    public class Client
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isGameOver = false;
        private bool _isMyTurn = false;

        public void Start()
        {
            _client = new TcpClient("127.0.0.1", 5000);
            _stream = _client.GetStream();
            Console.WriteLine("Connected to server. Waiting for the game to start...");

            // serverden gelen mesajlari qebul elemek ucun arxada ayri threadde isleyen metod cagirilir
            Task.Run(() => ListenForServerMessages());

            // esas thread input ucun istifade olunur
            while (!_isGameOver)
            {
                if (_isMyTurn)
                {
                    Console.Write("Enter your move (0-8): ");
                    string moveInput = Console.ReadLine();

                    // gedis servere gonderilir
                    byte[] moveData = Encoding.UTF8.GetBytes(moveInput);
                    _stream.Write(moveData, 0, moveData.Length);

                    // gedis bu oyuncudan alinir
                    _isMyTurn = false;
                }
            }

            _client.Close();
        }

        private void ListenForServerMessages()
        {
            while (!_isGameOver)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (message.StartsWith("BOARD:"))
                    {
                        DisplayBoard(message.Substring(6).Split(','));
                    }
                    else if (message.StartsWith("Game Over"))
                    {
                        Console.WriteLine(message);
                        _isGameOver = true;
                    }
                    else
                    {
                        Console.WriteLine(message);

                        if (message.Contains("Enter your move"))
                        {
                            _isMyTurn = true;
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Connection to server lost.");
                    _isGameOver = true;
                }
            }
        }

        private void DisplayBoard(string[] board)
        {
            Console.Clear();
            Console.WriteLine("Current Board:");
            for (int i = 0; i < 9; i++)
            {
                Console.Write(board[i] == " " ? "_" : board[i]);
                if ((i + 1) % 3 == 0) Console.WriteLine();
                else Console.Write("|");
            }
        }
    }
}
