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
        public bool? CurrentlyCorrect { get; set; }

        public Player(string username, string connectionId)
        {
            this.Username = username;
            this.ConnectionId = connectionId;
        }
    }
}
