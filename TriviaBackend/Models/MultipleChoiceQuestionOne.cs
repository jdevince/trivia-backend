using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TriviaBackend.Models
{
    public class MultipleChoiceQuestionOne
    {
        [JsonPropertyName("question")]
        public string Question { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        [JsonPropertyName("answer")]
        public string Answer { get; set; }
    }
}
