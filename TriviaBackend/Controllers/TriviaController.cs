using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TriviaBackend.Hubs;
using TriviaBackend.Services;
using static TriviaBackend.Models.Enums;

namespace TriviaBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TriviaController : ControllerBase
    {
        private readonly ILogger<TriviaController> _logger;
        private TriviaService _triviaService;

        public TriviaController(ILogger<TriviaController> logger, TriviaService triviaService)
        {
            _logger = logger;
            _triviaService = triviaService;
        }

        [HttpPost("start-game")]
        public ActionResult StartGame(Difficulty difficulty)
        {
            string newGameCode = _triviaService.CreateNewGame(difficulty);
            return Ok(newGameCode);
        }

        [HttpGet("keep-alive")]
        public ActionResult KeepAlive()
        {
            return Ok();
        }
    }
}
