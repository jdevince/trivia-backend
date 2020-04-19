using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaBackend.Models
{
    public class Player
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
        public int Score { get; set; }
        public string LastAnswer { get; set; }
        public bool LastAnswerCorrect { get; set; }
        public int VotesAsCorrectNum { get; set; }
        public int VotesAsCorrectDenom { get; set; }

        public Player(string username, string connectionId)
        {
            this.Username = username;
            this.ConnectionId = connectionId;
        }
    }
}
