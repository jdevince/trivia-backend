using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static TriviaBackend.Models.Enums;

namespace TriviaBackend.Models
{
    public class Question
    {
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public string Category { get; set; }
        public Difficulty Difficulty { get; set; }
        public List<string> Options { get; set; }

        public Question(string questionText, string answer, Difficulty difficulty, string category = null, List<string> options = null)
        {
            this.QuestionText = questionText;
            this.Answer = answer;
            this.Difficulty = difficulty;
            this.Category = category;
            this.Options = options;
        }

        public bool IsCorrect(string answer)
        {
            if (FormatAnswerForComparison(Answer) == FormatAnswerForComparison(answer))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string FormatAnswerForComparison(string input)
        {
            string output = input.ToLower();
            output = " " + output + " ";
            output = output.Replace(" the ", "");
            output = output.Replace(" a ", "");
            output = output.Replace(" an ", "");
            output = output.Replace(" ", "");

            return output;
        }
    }
}
