using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static TriviaBackend.Models.Enums;

namespace TriviaBackend.Models
{
    public class Game
    {
        public string GameCode { get; set; }
        public Difficulty Difficulty { get; set; }
        public bool IsStarted { get; set; }
        public string WinnerText { get; set; }
        public bool IsQuestionActive { get; set; }
        public List<Player> Players { get; set; }
        public Question CurrentQuestion { get; set; }
        public string LastAnswer { get; set; }

        public Game(string gameCode, Difficulty difficulty)
        {
            this.GameCode = gameCode;
            this.Difficulty = difficulty;
            this.Players = new List<Player>();
        }
    }
}
