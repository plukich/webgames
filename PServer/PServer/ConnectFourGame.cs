using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PServer
{
    public class ConnectFourGame : Game
    {
        private const int EMPTY = 0;
        private const int PLAYER1 = 1;
        private const int PLAYER2 = 2;

        private int turnCount = 0;
        private bool player1Turn = true;
        private int winner = -1;
        private int[][] board;
        private int player1Wins, player2Wins, draws;

        public ConnectFourGame(Player owner)
            : base(owner, GameType.CONNECT_FOUR)
        {
            maxPlayers = 2;
        }

        public override bool CanStart { get { return players.Count == 2; } }

        public override void Start()
        {
            base.Start();

            turnCount = 0;
            winner = -1;

            InitBoard();
        }

        private void InitBoard()
        {
            int i, j;
            board = new int[6][];
            for (i = 0; i < board.Length; i++)
            {
                board[i] = new int[7];
                for (j = 0; j < board[i].Length; j++)
                {
                    board[i][j] = EMPTY;
                }
            }
        }

        public bool TryMove(int this_player, int col)
        {
            if (!started)
            {
                Player player = null;
                lock (players)
                {
                    for (int j = 0; j < players.Count; j++)
                    {
                        if ((this_player - 1) == j)
                        {
                            player = players[j];
                            break;
                        }
                    }
                }

                if (player != null)
                {
                    player.Send(Server.Serialize(new ConnectFourMoveResponse { Success = false, Reason = "Move failed.  There is not currently a game in progress." }));
                }

                return false;
            }

            // returns true if valid move.
            bool valid = false;
            int i;
            for (i = board.Length - 1; i >= 0; i--)
            { // move up from bottom in specific column
                if (board[i][col] == EMPTY)
                {
                    valid = true;
                    break;
                }
            }

            if (valid)
            {
                board[i][col] = this_player;
                player1Turn = !player1Turn;
                turnCount++;

                // check for win (4 in a row)
                var gameOver = false;
                var gameOverAlert = "";
                if (checkForWin())
                {
                    if (winner == PLAYER2)
                    {
                        player2Wins++;
                        gameOverAlert = "Player2 wins!";
                    }
                    else
                    {
                        player1Wins++;
                        gameOverAlert = "Player1 win!";
                    }
                    gameOver = true;
                }
                else if (checkForDraw())
                {
                    draws++;
                    gameOver = true;
                    gameOverAlert = "Draw!";
                }

                if (gameOver)
                {
                    started = false;
                }

                lock (players)
                {
                    foreach (var player in players)
                    {
                        player.Send(Server.Serialize(new ConnectFourMoveResponse { PlayerNumber = this_player, Column = col, Success = true }));

                        if (gameOver)
                        {
                            player.Send(Server.Serialize(new ConnectFouGameOverMessage { GameID = id, WinnerPlayerNumber = this_player }));
                        }
                    }
                }

                return true; //valid move.
            }

            return false; // invalid move.
        }

        public int NextTurnPlayerNumber()
        {
            return player1Turn ? 1 : 2;
        }

        public bool checkForDraw()
        {
            int i, j;
            for (i = 0; i < board.Length; i++)
            {
                for (j = 0; j < board[i].Length; j++)
                {
                    if (board[i][j] == EMPTY) return false; // found an empty space, game not over.
                }
            }
            return true; // if we got here, the board is full.
        }

        public bool checkForWin()
        {
            // loop over rows and check for horizontal wins
            int i, j, this_player;
            for (i = 0; i < board.Length; i++)
            {
                for (j = 0; j < 4; j++)
                { // 4 possible horizontal wins in each row
                    this_player = board[i][j]; // get the player at the starting position of this 4-in-a-row.
                    if (this_player == EMPTY) continue;
                    if (board[i][j + 1] == this_player
                     && board[i][j + 2] == this_player
                     && board[i][j + 3] == this_player)
                    {
                        // win.
                        winner = this_player;
                        return true;
                    }
                }
            }

            // loop over columns and check for vertical wins
            for (i = 0; i < board[0].Length; i++)
            {
                for (j = 0; j < 3; j++)
                { // 3 possible vertical wins in each column
                    this_player = board[j][i]; // get the player at the starting position of this 4-in-a-row.
                    if (this_player == EMPTY) continue;
                    if (board[j + 1][i] == this_player
                     && board[j + 2][i] == this_player
                     && board[j + 3][i] == this_player)
                    {
                        // win.
                        winner = this_player;
                        return true;
                    }
                }
            }

            // loop over 4-block diagonal lines aiming DOWN-RIGHT (there are 12 possibilities)
            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    this_player = board[i][j]; // get the player at the starting position of this 4-in-a-row.
                    if (this_player == EMPTY) continue;
                    if (board[i + 1][j + 1] == this_player
                     && board[i + 2][j + 2] == this_player
                     && board[i + 3][j + 3] == this_player)
                    {
                        // win.
                        winner = this_player;
                        return true;
                    }
                }
            }

            // loop over 4-block diagonal lines aiming UP-RIGHT (or DOWN-LEFT) (there are 12 possibilities)
            for (i = 0; i < 3; i++)
            {
                for (j = 3; j < 7; j++)
                {
                    this_player = board[i][j]; // get the player at the starting position of this 4-in-a-row.
                    if (this_player == EMPTY) continue;
                    if (board[i + 1][j - 1] == this_player
                     && board[i + 2][j - 2] == this_player
                     && board[i + 3][j - 3] == this_player)
                    {
                        // win.
                        winner = this_player;
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
