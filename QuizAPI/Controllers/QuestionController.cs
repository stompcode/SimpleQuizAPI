using Microsoft.AspNetCore.Mvc;
using QuizAPI.Services;
using QuizAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp.Extensions;
using MongoDB.Bson;

namespace QuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly QuestionService _questionService;
        private readonly QuizService _quizService;
        private readonly UserService _userService;

        public QuestionController(QuestionService questionService,QuizService quizService, UserService userService)
        {
            _questionService = questionService;
            _quizService = quizService;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<List<Question>> Get() =>
            _questionService.Get();

        [HttpGet("{id:length(24)}", Name = "GetQuestion")]
        public ActionResult<Question> Get(string id)
        {
            var question = _questionService.Get(id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        [HttpPost]
        public ActionResult<Question> Create([Bind("quizId,TheQuestion,CorrectAnswer,FalseAnswers")] Question question)
        {
            var quiz = _quizService.Get(question.QuizId);
            if (quiz == null)
            {
                return NotFound();
            }
            else if (!isLoggedIn() || HttpContext.Request.Cookies["currentUser"] != quiz.CreatedById)
            {
                return Unauthorized();
            }
            question.QuizId = quiz.Id;
            if (quiz.Questions == null)
            {
                quiz.Questions = new List<String>();
            }
            _questionService.Create(question);
            quiz.Questions.Add(question.Id);
            _quizService.Update(question.QuizId, quiz);

            return CreatedAtRoute("GetUser", new { id = question.Id.ToString() }, question);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Question questionIn)
        {
            var question = _questionService.Get(id);
            var quiz = _quizService.Get(question.QuizId);
            if (question == null)
            {
                return NotFound();
            }
            else if (!isLoggedIn() || HttpContext.Request.Cookies["currentUser"] != quiz.CreatedById)
            {
                return Unauthorized();
            }

            _questionService.Update(id, questionIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var question = _questionService.Get(id);
            var quiz = _quizService.Get(question.QuizId);

            if (question == null)
            {
                return NotFound();
            }
            else if (!isLoggedIn() || HttpContext.Request.Cookies["currentUser"] != quiz.CreatedById)
            {
                return Unauthorized();
            }
            quiz.Questions.Remove(question.Id);
            _questionService.Remove(question.Id);
            _quizService.Update(quiz.Id, quiz);

            return NoContent();
        }

        private Boolean isLoggedIn()
        {
            String currentUser = HttpContext.Request.Cookies["currentUser"];
            var user = _userService.Get(currentUser);
            if (currentUser.HasValue() && user != null)
            {
                return true;
            }
            return false;
        }
    }
}
