using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TriviaBackend.Models;

namespace TriviaBackend.Services
{
    public class TriviaService
    {
        //Todo: Store in static memory for now, but probably needs to be in persistent storage (db)
        public List<Game> Games = new List<Game>();

        private List<Question> _questions = new List<Question>();
        private Random _random = new Random();

        public TriviaService()
        {
            AddJeopardyQuestions(); //~200,000
            AddMultipleChoicesQuestionsOne(); //~500
        }

        public string CreateNewGame()
        {
            string newGameCode = GetRandomGameCode();

            if (Games.Select(x => x.GameCode).Contains(newGameCode))
            {
                newGameCode = CreateNewGame();
            }
            else
            {
                Games.Add(new Game(newGameCode));
            }

            return newGameCode;
        }

        private string GetRandomGameCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 6;

            string newGameCode = new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());

            return newGameCode;
        }

        public Game GetCurrentGame(string connectionId)
        {
            foreach(Game game in Games)
            {
                foreach (Player player in game.Players)
                {
                    if (player.ConnectionId == connectionId)
                    {
                        return game;
                    }
                }
            }

            return null;
        }

        public Player GetCurrentPlayer(string connectionId)
        {
            foreach (Game game in Games)
            {
                foreach (Player player in game.Players)
                {
                    if (player.ConnectionId == connectionId)
                    {
                        return player;
                    }
                }
            }

            return null;
        }

        public Question GetQuestion()
        {
            int index = _random.Next(_questions.Count);
            Question question = _questions[index];
            return question;
        }

        private void AddJeopardyQuestions()
        {
            //Todo: Load from db
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonString = File.ReadAllText("JEOPARDY_QUESTIONS.json");
            var jeopardyQuestions = JsonSerializer.Deserialize<List<JeopardyQuestion>>(jsonString, options);

            foreach(JeopardyQuestion jeopardyQuestion in jeopardyQuestions)
            {
                if (!jeopardyQuestion.Answer.Contains("(") //Filter out weird formatted questions/answers
                    && !jeopardyQuestion.Answer.Contains('"')
                    && !jeopardyQuestion.Question.Contains("(")
                    && !jeopardyQuestion.Question.Contains("<"))
                {
                    string question = jeopardyQuestion.Question.Substring(1, jeopardyQuestion.Question.Length - 2); //Strip out starting + ending single quote
                    _questions.Add(new Question(question, jeopardyQuestion.Answer));
                }
            }
        }

        private void AddMultipleChoicesQuestionsOne()
        {
            //Todo: Load from db
            var jsonString = File.ReadAllText("MultipleChoiceQuestionsOne.json");
            var questions = JsonSerializer.Deserialize<List<MultipleChoiceQuestionOne>>(jsonString);

            foreach (MultipleChoiceQuestionOne question in questions)
            {                
                string answer = "";

                switch (question.Answer)
                {
                    case "A":
                        answer = question.A;
                        break;
                    case "B":
                        answer = question.B;
                        break;
                    case "C":
                        answer = question.C;
                        break;
                    case "D":
                        answer = question.D;
                        break;
                }

                answer = answer[0].ToString().ToUpper() + answer.Substring(1);
                
                Question newQuestion = new Question(question.Question, answer, new List<string>() { question.A, question.B, question.C, question.D });
                
                
                _questions.Add(newQuestion);
            }
        }
    }
}
