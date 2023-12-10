using Microsoft.AspNetCore.Mvc;
 using Microsoft.EntityFrameworkCore;
 using TP2_API.Config;
 using TP2_API.Models;
 using TP2_API.Data;
 using TP2_API.DTOs;
 using TP2_API.Utils;
 using TP2_API.Repository;
 
 
 [Route("api/[controller]")]
 [ApiController]
 public class UsersController : ControllerBase
 {
     private readonly UserRepository _userRepository;
 
     
     public UsersController()
     {
         // Initialize the DbContextOptions with the connection string from DatabaseConfig
         var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
             .UseNpgsql(DatabaseConfig.ConnectionString)
             .Options;
         var dbContext = new ApplicationDbContext(dbContextOptions);
 
         _userRepository = new UserRepository(dbContext);
     }
    
 
     [HttpPost]
     public async Task<IActionResult> CreateUser([FromBody] UserCreationDto userDto)
     {
         if (!ModelState.IsValid)
         {
             return BadRequest(ModelState);
         }
 
         try
         {
             // Before using userDto.Username and userDto.Password, check for nulls
             if (userDto.Username == null || userDto.Password == null)
             {
                 return BadRequest("Username and password are required.");
             }
             // Hash the user's password for your application
             var hashedPassword = PasswordHasher.HashPassword(userDto.Password);
         
             // Generate a secure password for PostgreSQL user
             var postgresPassword = PasswordGenerator.GenerateSecurePassword();
 
             // Encrypt the PostgreSQL password
             var encryptedPassword = EncryptionHelper.EncryptString(postgresPassword);
 
             // Create a new user object
             var user = new Users
             {
                 Username = userDto.Username,
                 PasswordHash = hashedPassword,
                 PostgresUsername = userDto.Username, 
                 PostgresPassword = encryptedPassword
             };
 
             // Use UserRepository to add the user
             await _userRepository.AddUser(user);
             
             // Prepare the response
             var createdUserDto = new UserDto
             {
                 UserId = (int)user.UserId,
                 Username = user.Username
             };
 
             return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, createdUserDto);
         }
         catch (Exception ex)
         {
             // Handle any exceptions
             return StatusCode(500, $"An error occurred: {ex.Message}");
         }
     }
 
     
     [HttpPost("login")]
     public async Task<IActionResult> Login(LoginDto loginDto)
     {
         var user = await _userRepository.AuthenticateUser(loginDto.Username, loginDto.Password);
 
         if (user == null)
         {
             return Unauthorized("Invalid username or password.");
         }
 
         // Decrypt the PostgreSQL password
         var postgresPassword = EncryptionHelper.DecryptString(user.PostgresPassword);
 
         // Prepare the response with PostgreSQL credentials
         var loginResponse = new
         {
             Message = "Login successful",
             UserId = user.UserId,
             Username = user.Username,
             isAdmin = user.IsAdmin,
             PostgresUsername = user.PostgresUsername,
             PostgresPassword = postgresPassword
         };
 
         return Ok(loginResponse);
     }
 
 
 
 
 
 
     // Endpoint to promote a user to admin
     [HttpPut("promote/{username}")]
     public async Task<IActionResult> PromoteToAdmin(string username, [FromBody] string requesterUsername)
     {
         var result = await _userRepository.PromoteUserToAdmin(username, requesterUsername);
         if (result)
         {
             return Ok($"{username} has been promoted to admin.");
         }
         return NotFound($"User {username} not found or already an admin.");
     }

 
     // DELETE: api/users/{userId}
[HttpDelete("{userId}")]
public async Task<IActionResult> DeleteUser(int userId, [FromBody] string requesterUsername)
{
    var deleteResult = await _userRepository.DeleteUser(userId, requesterUsername);
    
    if (deleteResult == DeleteUserResult.NotAuthorized)
    {
        return Unauthorized("Only admins can delete users.");
    }
    else if (deleteResult == DeleteUserResult.UserNotFound || deleteResult == DeleteUserResult.NotInactiveLongEnough)
    {
        return NotFound("User not found or not inactive for 7 days.");
    }
    else if (deleteResult == DeleteUserResult.Success)
    {
        return Ok($"User with ID {userId} has been deleted.");
    }

    return StatusCode(500, "An error occurred while processing the request.");
}


     // GET: api/users/{id}
     [HttpGet("{id}")]
     public async Task<ActionResult<UserDto>> GetUserById(int id)
     {
         // Retrieve the user using UserRepository
         var user = await _userRepository.GetUserById(id);
 
         // Handle the case where the user is not found
         if (user == null)
         {
             return NotFound();
         }
 
         // Prepare and return the UserDto
         var userDto = new UserDto
         {
             UserId = (int)user.UserId,
             Username = user.Username,
             // Map other fields as necessary
         };
 
         return userDto; // Return the UserDto
     }
 }