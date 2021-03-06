﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TriviaBackend.Models;
using static TriviaBackend.Models.Enums;

namespace TriviaBackend.Services
{
    public class TriviaService
    {
        //Todo: Store in static memory for now, but probably needs to be in persistent storage (db)
        public List<Game> Games = new List<Game>();

        private List<Question> _questions = new List<Question>();
        private Random _random = new Random();
        static readonly HttpClient _httpClient = new HttpClient();

        public TriviaService()
        {
            AddJeopardyQuestions(); //~200,000
            AddMultipleChoicesQuestionsOne(); //~500
        }

        public string CreateNewGame(Difficulty difficulty)
        {
            string newGameCode = GetRandomGameCode();

            if (Games.Select(x => x.GameCode).Contains(newGameCode))
            {
                newGameCode = CreateNewGame(difficulty);
            }
            else
            {
                Games.Add(new Game(newGameCode, difficulty));
            }

            return newGameCode;
        }

        private string GetRandomGameCode()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int length = 3;

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

        public void DeleteGame(Game game)
        {
            Games.Remove(game);
        }

        public void CheckForStaleGames()
        {
            int gameCount = Games.Count;
            for (int i = 0; i < gameCount; i++)
            {
                if (i < Games.Count)
                {
                    Game game = Games[i];
                    if (game.Players == null || game.Players.Count == 0)
                    {
                        DeleteGame(game);
                        i--;
                        gameCount--;
                    }
                }
            }
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

        public Player GetPlayer(Game game, string username)
        {
            foreach (Player player in game.Players)
            {
                if (player.Username == username)
                {
                    return player;
                }
            }

            return null;
        }

        public async Task<Question> GetQuestion(Difficulty difficulty)
        {
            Question question = null;
            if (difficulty == Difficulty.Easy && _random.Next(4000) > 500) //~500 easy from json and 3500 from Open Trivia DB
            {
                question = await GetQuestionFromOpenTriviaDB();
            }

            if (question == null)
            {
                List<Question> tempQuestions = _questions.Where(x => x.Difficulty == difficulty).ToList();
                int index = _random.Next(tempQuestions.Count);
                question = tempQuestions[index];
            }
            
            return question;
        }

        private async Task<Question> GetQuestionFromOpenTriviaDB()
        {
            Question question = null;
            
            //Try to get from Open Trivia DB via API
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://opentdb.com/api.php?amount=1");
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    OpenTriviaDBAPIResponse responseObj = JsonSerializer.Deserialize<OpenTriviaDBAPIResponse>(jsonString, jsonOptions);
                    OpenTriviaDBQuestion openTriviaDBQuestion = responseObj.Results.First();

                    List<string> options = openTriviaDBQuestion.IncorrectAnswers.Append(openTriviaDBQuestion.CorrectAnswer).ToList();
                    options.ForEach(x => x = HttpUtility.HtmlDecode(x));
                    options.Sort();
                    
                    question = new Question(
                                    HttpUtility.HtmlDecode(openTriviaDBQuestion.Question),
                                    HttpUtility.HtmlDecode(openTriviaDBQuestion.CorrectAnswer), 
                                    Difficulty.Easy,
                                    HttpUtility.HtmlDecode(openTriviaDBQuestion.Category), 
                                    options);
                }
            }
            catch (Exception e)
            {
                question = null; //Use failsafe from non-API questions
            }

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
                    Difficulty difficulty = Difficulty.Medium; //Top 2 rows of Jeopardy questions are medium

                    if (jeopardyQuestion.Round == "Jeopardy!")
                    {
                         if (jeopardyQuestion.Value == "$600" || jeopardyQuestion.Value == "$800" || jeopardyQuestion.Value == "$1000")
                        {
                            difficulty = Difficulty.Hard;
                        }
                    }
                    else if (jeopardyQuestion.Round == "Double Jeopardy!")
                    {
                        if (jeopardyQuestion.Value == "$1200" || jeopardyQuestion.Value == "$1600" || jeopardyQuestion.Value == "$2000")
                        {
                            difficulty = Difficulty.Hard;
                        }
                    }

                    string question = jeopardyQuestion.Question.Substring(1, jeopardyQuestion.Question.Length - 2); //Strip out starting + ending single quote
                    _questions.Add(new Question(question, jeopardyQuestion.Answer, difficulty, jeopardyQuestion.Category));
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

                answer = Capitalize(answer);
                question.A = Capitalize(question.A);
                question.B = Capitalize(question.B);
                question.C = Capitalize(question.C);
                question.D = Capitalize(question.D);

                Question newQuestion = new Question(question.Question, answer, Difficulty.Easy, options: new List<string>() { question.A, question.B, question.C, question.D });
                
                _questions.Add(newQuestion);
            }
        }

        private string Capitalize(string input)
        {
            string output = input[0].ToString().ToUpper() + input.Substring(1);
            return output;
        }
    }
}
