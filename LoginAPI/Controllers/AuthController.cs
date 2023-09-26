using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LoginAPI.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace LoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Static dictionary to store registered users in-memory
        private static Dictionary<string, User> _users = new Dictionary<string, User>();

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Endpoint for user registration
        [HttpPost("register")]
        public ActionResult<User> Register(UserDto request)
        {
            // Hash the incoming password using BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create a new User object with the provided username and hashed password
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };

            // Add the new user to the in-memory storage of users
            _users[request.Username] = newUser;

            // Log the registration of the new user
            Console.WriteLine($"User registered: {newUser.Username}");

            // Return the registered user object
            return Ok(newUser);
        }

        // Endpoint for user login
        [HttpPost("login")]
        public ActionResult<string> Login(UserDto request)
        {
            // Log the login attempt
            Console.WriteLine($"Login attempt for user: {request.Username}");

            // Try to retrieve the stored user from the in-memory storage by username
            if (!_users.TryGetValue(request.Username, out var storedUser))
            {
                // If the user is not found, return a BadRequest response
                return BadRequest("User not found");
            }

            // Log the stored username
            Console.WriteLine($"Stored username: {storedUser.Username}");

            // Verify the provided password against the stored password hash using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, storedUser.PasswordHash))
            {
                // If the password verification fails, return a BadRequest response
                return BadRequest("Wrong password.");
            }

            // Create a JWT token for the authenticated user
            string token = CreateToken(storedUser);

            // Return the JWT token to the client
            return Ok(token);
        }

        // Helper method to create a JWT token for a user
        private string CreateToken(User user)
        {
            // Define claims for the JWT token (in this case, the user's name)
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            // Generate a symmetric security key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            // Create signing credentials using the security key and HMAC-SHA512 signature algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create a JWT token with the specified claims, expiration, and signing credentials
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),  // Token expiration time 
                signingCredentials: creds
            );

            // Serialize the JWT token into a string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Return the JWT token
            return jwt;
        }
    }
}
