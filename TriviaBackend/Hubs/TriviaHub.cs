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

            gameCode = gameCode.ToUpper();
            Game game = _triviaService.Games.FirstOrDefault(x => x.GameCode == gameCode);

            if (game != null)
            {
                game.Players.Add(player);
                SendDataToPlayers(game, "gameStateChange", game);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GetCurrentGameState()
        {
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            _hubContext.Clients.Client(Context.ConnectionId).SendAsync("gameStateChange", game);
        }

        public void LeaveGame()
        {
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            Player player = _triviaService.GetCurrentPlayer(Context.ConnectionId);
            game.Players.Remove(player);

            if (game.Players.Count == 0)
            {
                _triviaService.DeleteGame(game);
            }
            else
            {
                SendDataToPlayers(game, "gameStateChange", game);
            }
        }

        public void StartGame()
        {
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            ResetGame(game);
            new Task(() => { PlayGame(game); }).Start();
        }

        private void ResetGame(Game game)
        {
            game.IsStarted = true;
            game.WinnerText = null;
            game.Players.ForEach(player => {
                player.Score = 0;
            });
            SendDataToPlayers(game, "gameStateChange", game);
        }

        private void ResetQuestion(Game game)
        {
            int votesAsCorrectDenom = 3;
            if (game.Players.Count < 3)
            {
                votesAsCorrectDenom = game.Players.Count;
            }

            game.Players.ForEach(player => {
                player.LastAnswer = null;
                player.LastAnswerCorrect = false;
                player.VotesAsCorrectNum = 0;
                player.VotesAsCorrectDenom = votesAsCorrectDenom;
            });

            game.IsQuestionActive = true;

            SendDataToPlayers(game, "gameStateChange", game);
        }

        public void PlayGame(Game game)
        {
            bool gameOver = false;
            while (!gameOver)
            {
                ResetQuestion(game);

                //Get a new question
                Question newQuestion = _triviaService.GetQuestion(game.Difficulty);
                game.CurrentQuestion = newQuestion;
                SendDataToPlayers(game, "newQuestion", newQuestion);

                //Every half-second, check if all players have responded. End after 30 seconds.
                for (int i = 0; i < 60; i++)
                {
                    Thread.Sleep(500);
                    if (game.Players.All(x => !string.IsNullOrWhiteSpace(x.LastAnswer)))
                    {
                        break;
                    }
                }
                SendDataToPlayers(game, "endQuestion");

                game.Players.Where(x => string.IsNullOrWhiteSpace(x.LastAnswer)).ToList().ForEach(player => {
                    player.LastAnswer = "None";
                    player.LastAnswerCorrect = false;
                });
                game.LastAnswer = newQuestion.Answer;
                game.IsQuestionActive = false;

                if (game.Players.FirstOrDefault(x => x.Score >= 100) != null)
                {
                    gameOver = true;
                    break;
                }

                if (game.Players.Count == 0)
                {
                    return;
                }

                SendDataToPlayers(game, "gameStateChange", game);

                //Wait 15 seconds before next question
                Thread.Sleep(15000);
            }

            game.IsStarted = false;

            int topScore = game.Players.OrderByDescending(x => x.Score).First().Score;
            List<Player> winners = game.Players.Where(x => x.Score == topScore).ToList();
            if (winners.Count == 1)
            {
                game.WinnerText = string.Format("{0} wins!", winners[0].Username);
            }
            else
            {
                for(int i = 0; i < winners.Count; i++)
                {
                    if (i == 0)
                    {
                        game.WinnerText += winners[i].Username;
                    }
                    else
                    {
                        game.WinnerText += " and " + winners[i].Username;
                    }
                }

                game.WinnerText += " win!";
            }

            SendDataToPlayers(game, "gameStateChange", game);
        }

        public bool AnswerQuestion(string answer)
        {
            bool result;
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            Player player = _triviaService.GetCurrentPlayer(Context.ConnectionId);

            if (game.CurrentQuestion.IsCorrect(answer))
            {
                result = true;
                player.Score += 10 - game.Players.Where(x => x.LastAnswerCorrect).Count();
            }
            else
            {
                result = false;
            }

            player.LastAnswer = answer;
            player.LastAnswerCorrect = result;

            SendDataToPlayers(game, "gameStateChange", game);

            return result;
        }

        private void SendDataToPlayers(Game game, string method, object data = null)
        {
            List<string> connectionIds = game.Players.Select(x => x.ConnectionId).ToList();

            if (connectionIds.Count > 0)
            {
                _hubContext.Clients.Clients(connectionIds).SendAsync(method, data);
            }
        }

        public void VoteAsCorrect(string username)
        {
            Game game = _triviaService.GetCurrentGame(Context.ConnectionId);
            Player player = _triviaService.GetPlayer(game, username);
            player.VotesAsCorrectNum++;

            if (player.VotesAsCorrectNum >= player.VotesAsCorrectDenom)
            {
                player.Score += 10;
                player.LastAnswerCorrect = true;
            }

            SendDataToPlayers(game, "gameStateChange", game);
        }
    }
}
