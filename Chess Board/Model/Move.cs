namespace Chess_Board.Models
{
    public class Move
    {
        public int FromRow;
        public int FromCol;

        public int ToRow;
        public int ToCol;

        public Move(int fr, int fc, int tr, int tc)
        {
            FromRow = fr;
            FromCol = fc;

            ToRow = tr;
            ToCol = tc;
        }
    }
}
