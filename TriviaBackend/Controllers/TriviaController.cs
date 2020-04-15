using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TriviaBackend.Hubs;

namespace TriviaBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TriviaController : ControllerBase
    {
        private readonly ILogger<TriviaController> _logger;
        private TriviaHub _triviaHub;

        public TriviaController(ILogger<TriviaController> logger, TriviaHub triviaHub)
        {
            _logger = logger;
            _triviaHub = triviaHub;
        }

        [HttpPost("start-game")]
        public bool StartGame()
        {
            _triviaHub.BroadcastMessage("name", "mes");
            return true;
        }
    }
}
