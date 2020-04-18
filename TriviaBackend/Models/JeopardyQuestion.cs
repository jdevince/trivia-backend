using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TriviaBackend.Models
{
    public class JeopardyQuestion
    {
        [JsonPropertyName("show_number")]
        public string ShowNumber { get; set; }
        [JsonPropertyName("air_date")]
        public string AirDate { get; set; }
        public string Round { get; set; }
        public string Category { get; set; }
        public string Value { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
