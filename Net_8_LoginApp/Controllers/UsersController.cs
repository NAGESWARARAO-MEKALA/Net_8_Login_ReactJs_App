using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net_8_LoginApp.Models;

namespace Net_8_LoginApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDbContext dbContext;
        public UsersController(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        [Route("Registration")]       
        public IActionResult Registration(UserDTO userDTO)
        {
            // Ensure that the model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Ensure Email is not null or empty
            if (string.IsNullOrEmpty(userDTO.Email))
            {
                return BadRequest("Email cannot be null or empty.");
            }

            // Validate email format
            if (!IsValidEmail(userDTO.Email))
            {
                return BadRequest("Invalid email format.");
            }

            // Convert the email to lowercase for a case-insensitive comparison
            var emailLower = userDTO.Email.ToLower();

            // Safely query the Users table to check if the email already exists
            var objUser = dbContext.Users.FirstOrDefault(x => x.Email != null && x.Email.ToLower() == emailLower);

            if (objUser == null)
            {
                // Add new user to the Users table
                var newUser = new User
                {
                    FirstName = userDTO.FirstName ?? string.Empty,  // Handle null values with default empty strings
                    LastName = userDTO.LastName ?? string.Empty,
                    Email = userDTO.Email,
                    PasswordHash = userDTO.PasswordHash ?? string.Empty,    // Hash the password before storing in real-world applications
                    CreatedOn = DateTime.Now
                };

                try
                {
                    dbContext.Users.Add(newUser);
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    // Handle potential exceptions during database operations
                    return StatusCode(500, $"An error occurred while saving the user: {ex.Message}");
                }

                return Ok("Registered Successfully");
            }
            else
            {
                return BadRequest("User already exists with the same email address.");
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }





        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginDTO loginDTO)
        {
            var User = dbContext.Users.FirstOrDefault(x=>x.Email==loginDTO.Email && x.PasswordHash == loginDTO.PasswordHash);
            if (User != null)
            {
                return Ok(User);
            }
            return NoContent();
        }
        [HttpGet]
        [Route("GetUsers")]
        public IActionResult GetUsers()
        {
            return Ok(dbContext.Users.ToList());
        }
        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser(int id) {
            var user = dbContext.Users.FirstOrDefault(x => x.Id == id);
            if (user != null)
            {
                return Ok();
            }
            return NoContent();
        }
    }

}
