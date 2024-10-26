using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeTCP
{
    public class Server
    {
        private TcpListener _listener;
        private TcpClient[] _clients = new TcpClient[2];
        private char[] _board = new char[9];
        private int _currentPlayer = 0; // 0  -  X, 1  -  O (oyuncunun isaresi)
        private bool _isGameOver = false;

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 5000);
            _listener.Start();
            Console.WriteLine("Server started. Waiting for clients...");

            // iki client gozleyir server
            for (int i = 0; i < 2; i++)
            {
                _clients[i] = _listener.AcceptTcpClient();
                Console.WriteLine($"Client {i + 1} connected.");
            }

            // oyun "masasi" dolur
            Array.Fill(_board, ' ');
            BroadcastBoard();
            StartGame();
        }

        private void StartGame()
        {
            while (!_isGameOver)
            {
                if (_currentPlayer == 0)
                {
                    BroadcastMessage("Player X's turn. Enter your move (0-8): ");
                }
                else
                {
                    BroadcastMessage("Player O's turn. Enter your move (0-8): ");
                }

                // hal hazirda gedisi olan oyuncunun gedisini gozleyir
                string move = WaitForMove(_clients[_currentPlayer]);
                if (IsValidMove(move, out int moveIndex))
                {
                    _board[moveIndex] = _currentPlayer == 0 ? 'X' : 'O';
                    BroadcastBoard();
                    CheckForWinner();
                    _currentPlayer = (_currentPlayer + 1) % 2; // gedisin sira deyismesi
                }
                else
                {
                    BroadcastMessage("Invalid move. Try again.");
                }
            }
        }

        private string WaitForMove(TcpClient client)
        {
            byte[] buffer = new byte[1024];
            NetworkStream stream = client.GetStream();
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private bool IsValidMove(string move, out int index)
        {
            if (int.TryParse(move, out index) && index >= 0 && index < 9 && _board[index] == ' ')
            {
                return true;
            }
            index = -1;
            return false;
        }

        private void CheckForWinner()
        {
            int[,] winningCombinations = new int[,]
            {
                { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, // ufuqi
                { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, // saquli
                { 0, 4, 8 }, { 2, 4, 6 } // diaqonal
            };

            for (int i = 0; i < winningCombinations.GetLength(0); i++)
            {
                if (_board[winningCombinations[i, 0]] != ' ' &&
                    _board[winningCombinations[i, 0]] == _board[winningCombinations[i, 1]] &&
                    _board[winningCombinations[i, 1]] == _board[winningCombinations[i, 2]])
                {
                    BroadcastMessage($"Player {_board[winningCombinations[i, 0]]} wins!");
                    _isGameOver = true;
                    return;
                }
            }

            // hechece
            if (Array.TrueForAll(_board, c => c != ' '))
            {
                BroadcastMessage("It's a draw!");
                _isGameOver = true;
            }
        }

        private void BroadcastBoard()//masani gosterir
        {
            string boardMessage = "BOARD:" + string.Join(",", _board);
            foreach (var client in _clients)
            {
                SendMessage(client, boardMessage);
            }
        }

        private void BroadcastMessage(string message)
        {
            foreach (var client in _clients)
            {
                SendMessage(client, message);
            }
        }

        private void SendMessage(TcpClient client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

    }
}
