using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaBackend.Models
{
    public class Question
    {
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public List<string> Options { get; set; }
        

        public Question(string questionText, string answer, List<string> options = null)
        {
            this.QuestionText = questionText;
            this.Answer = answer;
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
