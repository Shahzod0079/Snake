using System.Collections.Generic;

namespace Common
{
    public class ViewModelGames
    {
        public Snakes SnakesPlayers { get; set; } = new Snakes();
        public List<Snakes> AllSnakes { get; set; } = new List<Snakes>();  
        public Snakes.Point Points { get; set; } = new Snakes.Point();
        public int Top { get; set; }
        public int IdSnake { get; set; }
    }
}