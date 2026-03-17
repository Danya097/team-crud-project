using System.Data.SQLite;

namespace Chess_Board.Data
{
    public class ChessDbHelper
    {
        string db = "Data Source=chess.db";

        public void SaveGame(string fen)
        {
            using var con = new SQLiteConnection(db);

            con.Open();

            string sql = "INSERT INTO Games (FEN) VALUES (@fen)";

            using var cmd = new SQLiteCommand(sql, con);

            cmd.Parameters.AddWithValue("@fen", fen);

            cmd.ExecuteNonQuery();
        }

        public string LoadLastGame()
        {
            using var con = new SQLiteConnection(db);

            con.Open();

            string sql = "SELECT FEN FROM Games ORDER BY Id DESC LIMIT 1";

            using var cmd = new SQLiteCommand(sql, con);

            var result = cmd.ExecuteScalar();

            return result?.ToString();
        }
    }
}
