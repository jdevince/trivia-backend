using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TriviaBackend.Models
{
    public class OpenTriviaDBAPIResponse
    {
        [JsonPropertyName("response_code")]
        public int ResponseCode { get; set; }
        public IEnumerable<OpenTriviaDBQuestion> Results { get; set; }
    }
}
