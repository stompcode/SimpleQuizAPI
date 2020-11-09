using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using QuizAPI.Models;
using QuizAPI.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Data;
using System.Web;
using Microsoft.AspNetCore.Http;
using RestSharp.Extensions;
using MailKit.Net.Smtp;
using MimeKit;

namespace QuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Authentication _auth;
        private readonly UserService _userService;
        private readonly QuizService _quizService;

        public UserController(UserService userService, Authentication auth, QuizService quizService)
        {
            _quizService = quizService;
            _auth = auth;
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<List<User>> Get() =>
            _userService.Get();

        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<User> Get(string id)
        {
            var user = _userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public ActionResult<User> Create([Bind("username,passwordHash")] User user)
        {
            string confirmationCode = GenerateConfirmationCode();
            var userResult = _userService.GetByUserName(user.Username);
            if (userResult != null)
            {
                return Conflict();
            }
            user.PasswordHash = GetHashed(user.PasswordHash);
            user.ConfirmationCode = confirmationCode;
            _userService.Create(user);
            SendConfirmationEmail(user.Username, user.Email, confirmationCode);

            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        [HttpPost]
        [Route("Login")]
        public ActionResult Login([Bind("username,passwordHash")] User user)
        {
            var userResult = _userService.GetByUserName(user.Username);
            if (userResult == null)
            {
                return NotFound();
            } 
            else
            {
                if (GetHashed(user.PasswordHash).Equals(userResult.PasswordHash))
                {
                    HttpContext.Response.Cookies.Append(
                        "currentUser", userResult.Id, new CookieOptions(){Expires = DateTime.UtcNow.AddHours(1)});
                    return StatusCode(200);
                } 
                else 
                {
                    return Unauthorized();
                }
            }
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, User userIn)
        {
            var user = _userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }
            else if (!IsLoggedIn() || (HttpContext.Request.Cookies["currentUser"]) != id)
            {
                return Unauthorized();
            }

            _userService.Update(id, userIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var user = _userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }
            else if (!IsLoggedIn() || (HttpContext.Request.Cookies["currentUser"]) !=id)
            {
                return Unauthorized();
            }

            _userService.Remove(user.Id);

            return NoContent();
        }

        [HttpPut]
        [Route("ConfirmEmail")]
        public IActionResult ConfirmEmail(string confirmationCode)
        {
            List<User> users = _userService.Get();
            foreach (User user in users)
            {
                if (user.ConfirmationCode == confirmationCode)
                {
                    user.Confirmed = true;
                    _userService.Update(user.Id, user);
                    return Ok();
                }
            }
            return BadRequest();
        }

        [HttpPut]
        [Route("AddFavorite")]
        public IActionResult AddFavorite(string userId, string quizId)
        {
            User user = _userService.Get(userId);
            Quiz quiz = _quizService.Get(quizId);

            user.FavoritesQuizId.Add(quiz.Id);
            quiz.Favorited++;
            _userService.Update(user.Id, user);
            _quizService.Update(quiz.Id, quiz);
            return Ok();
        }
        [HttpPut]
        [Route("RemoveFavorite")]
        public IActionResult RemoveFavorite(string userId, string quizId)
        {
            User user = _userService.Get(userId);
            Quiz quiz = _quizService.Get(quizId);

            user.FavoritesQuizId.Remove(quiz.Id);
            quiz.Favorited--;
            _userService.Update(user.Id, user);
            _quizService.Update(quiz.Id, quiz);
            return Ok();
        }

        private string GetHashed(string password)
        {

            var salt = new byte[int.Parse(_auth.PassSalt)];
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

            return hashed;
        }

        private Boolean IsLoggedIn()
        {
            String currentUser = HttpContext.Request.Cookies["currentUser"];
            var user = _userService.Get(currentUser);
            if (currentUser.HasValue() && user != null)
            {
                return true;
            }
            return false;
        }

        private void SendConfirmationEmail(string username, string email, string confirmationCode)
        {
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("QuizApp", "testingapp410@gmail.com"));
            message.To.Add(new MailboxAddress(username, email));
            message.Subject = "Email Confirmation";

            message.Body = new TextPart("plain")
            {
                Text = "Hello, This is your confirmation Code: " + confirmationCode
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("testingapp410@gmail.com", _auth.EmailPass);

                client.Send(message);
                client.Disconnect(true);
            }
        }
        
        private string GenerateConfirmationCode()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
