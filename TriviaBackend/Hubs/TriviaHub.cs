using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TriviaBackend.Models;
using TriviaBackend.Services;

namespace TriviaBackend.Hubs
{
    public class TriviaHub : Hub
    {
        private readonly IHubContext<TriviaHub> _hubContext;
        private TriviaService _triviaService;

        public TriviaHub(TriviaService triviaService, IHubContext<TriviaHub> hubContext)
        {
            _triviaService = triviaService;
            _hubContext = hubContext;
        }

        public bool JoinGame(string username, string gameCode)
        {
            Player player = new Player(username, Context.ConnectionId);

            Game game = _triviaService.Games.FirstOrDefault(x => x.GameCode == gameCode);

            if (game != null)
            {
                game.Players.Add(player);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateGameState()
        {
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            SendDataToPlayers(game, "gameStateChange", game);
        }

        public void StartGame()
        {
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            new Task(() => { PlayGame(game); }).Start();
        }

        public void PlayGame(Game game)
        {
            bool gameOver = false;
            while (!gameOver)
            {
                //Todo: Add check if anyone's still connected

                //Reset all players responses to blank
                game.Players.ForEach(player => player.CurrentlyCorrect = null);

                //Get a new question
                Question newQuestion = _triviaService.GetQuestion();
                game.CurrentQuestion = newQuestion;
                SendDataToPlayers(game, "newQuestion", newQuestion);

                //Every half-second, check if all players have responded. End after 30 seconds.
                for (int i = 0; i < 60; i++)
                {
                    Thread.Sleep(500);
                    if (game.Players.All(x => x.CurrentlyCorrect != null))
                    {
                        break;
                    }
                }
                SendDataToPlayers(game, "endQuestion", game.CurrentQuestion.Answer);

                SendDataToPlayers(game, "gameStateChange", game);

                if (game.Players.FirstOrDefault(x => x.Score >= 100) != null)
                {
                    gameOver = true;
                    break;
                }

                //Wait 10 seconds before next question
                Thread.Sleep(10000);
            }
        }

        public bool AnswerQuestion(string answer)
        {
            bool result;
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            Player player = _triviaService.GetCurrentPlayer(Context.ConnectionId);

            if (game.CurrentQuestion.IsCorrect(answer))
            {
                result = true;
                player.Score += 10 - game.Players.Where(x => x.CurrentlyCorrect == true).Count();
            }
            else
            {
                result = false;
            }

            player.CurrentlyCorrect = result;

            SendDataToPlayers(game, "gameStateChange", game);

            return result;
        }

        private void SendDataToPlayers(Game game, string method, object data = null)
        {
            List<string> connectionIds = game.Players.Select(x => x.ConnectionId).ToList();
            _hubContext.Clients.Clients(connectionIds).SendAsync(method, data);
        }
    }
}
