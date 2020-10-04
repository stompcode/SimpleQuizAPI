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

namespace QuizAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Authentication _auth;
        private readonly UserService _userService;

        public UserController(UserService userService, Authentication auth)
        {
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
            var userResult = _userService.GetByUserName(user.Username);
            if (userResult != null)
            {
                return Conflict();
            }
            user.PasswordHash = getHashed(user.PasswordHash);
            _userService.Create(user);

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
                if (getHashed(user.PasswordHash).Equals(userResult.PasswordHash))
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
            else if (!isLoggedIn() || (HttpContext.Request.Cookies["currentUser"]) != id)
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
            else if (!isLoggedIn() || (HttpContext.Request.Cookies["currentUser"]) !=id)
            {
                return Unauthorized();
            }

            _userService.Remove(user.Id);

            return NoContent();
        }

        private string getHashed(string password)
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
