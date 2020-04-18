using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaBackend.Models
{
    public class Game
    {
        public string GameCode { get; set; }
        public List<Player> Players { get; set; }
        public Question CurrentQuestion { get; set; }

        public Game(string gameCode)
        {
            this.GameCode = gameCode;
            this.Players = new List<Player>();
        }
    }
}
