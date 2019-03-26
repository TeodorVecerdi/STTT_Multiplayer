namespace TeodorVecerdi {
    public class Board {
        private int[,] board;
        private int n;
        private int winner;
        private int moves;

        public Board() {
            Reset();
        }

        private void Reset() {
            board = new[,] {{0, 0, 0}, {0, 0, 0}, {0, 0, 0}};
            n = 3;
            winner = 0;
            moves = 0;
        }

        private int col, row, diag, rdiag; // used for checking if the player won after making a move

        public (int code, string message) TryMakeMove(int x, int y, int player) {
            if (x < 0 || x >= n || y < 0 || y >= n)
                return (-2, $"Illegal move; Cell {x},{y} outside of board");
            if (board[x, y] != 0)
                return (-2, $"Illegal move; Cell {x},{y} is already taken by player {board[x, y]}");

            board[x, y] = player;
            moves++;

            // check if the player won
            col = row = diag = rdiag = 0;
            for (int i = 0; i < n; i++) {
                if (board[x, i] == player) col++;
                if (board[i, y] == player) row++;
                if (board[i, i] == player) diag++;
                if (board[i, n - i - 1] == player) rdiag++;

                if (row == n || col == n || diag == n || rdiag == n) {
                    winner = player;
                    return (player, null);
                }
            }

            return (0, null);
        }

        public bool Finished => winner != 0;
    }
}