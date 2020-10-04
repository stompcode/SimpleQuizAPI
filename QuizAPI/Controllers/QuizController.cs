using Microsoft.AspNetCore.Mvc;
using QuizAPI.Services;
using QuizAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RestSharp.Extensions;

namespace QuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly QuizService _quizService;
        private readonly UserService _userService;
        private readonly QuestionService _questionService;

        public QuizController(QuizService quizService, UserService userService, QuestionService questionService)
        {
            _quizService = quizService;
            _userService = userService;
            _questionService = questionService;
        }
        [HttpGet]
        public ActionResult<List<Quiz>> Get() =>
            _quizService.Get();

        [HttpGet("{id:length(24)}", Name = "GetQuiz")]
        public ActionResult<Quiz> Get(string id)
        {
            var quiz = _quizService.Get(id);

            if (quiz == null)
            {
                return NotFound();
            }

            return quiz;
        }

        [HttpPost]
        public ActionResult<Quiz> Create([Bind("QuizName")] Quiz quiz)
        {
            if (!isLoggedIn())
            {
                return Unauthorized();
            }
            Models.User user = _userService.Get(HttpContext.Request.Cookies["currentUser"]);
            if (user.QuizIdList == null)
            {
                user.QuizIdList = new List<String>();
            }

            quiz.CreatedById = user.Id;
            _quizService.Create(quiz);
            user.QuizIdList.Add(_quizService.Get(quiz.Id).Id);

            _userService.Update(user.Id, user);

            return CreatedAtRoute("GetQuiz", new { id = quiz.Id.ToString() }, quiz);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Quiz quizIn)
        {
            var quiz = _quizService.Get(id);

            if (quiz == null)
            {
                return NotFound();
            }
            else if (!isLoggedIn() || quiz.CreatedById != HttpContext.Request.Cookies["currentUser"]) 
            {
                return Unauthorized();
            }

            _quizService.Update(id, quizIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var quiz = _quizService.Get(id);

            if (quiz == null)
            {
                return NotFound();
            }
            else if (!isLoggedIn() || quiz.CreatedById != HttpContext.Request.Cookies["currentUser"])
            {
                return Unauthorized();
            }
            User user = _userService.Get(HttpContext.Request.Cookies["currentUser"]);

            _quizService.Remove(quiz.Id);

            user.QuizIdList.Remove(quiz.Id);
            _userService.Update(user.Id, user);

            return NoContent();
        }
        private Boolean isLoggedIn()
        {
            if (HttpContext.Request.Cookies["currentUser"].HasValue())
            {
                return true;
            }
            return false;
        }
    }
}
