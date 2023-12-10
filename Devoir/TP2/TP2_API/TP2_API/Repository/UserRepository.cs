using Microsoft.EntityFrameworkCore;
using Npgsql;
using TP2_API.Data;
using TP2_API.DTOs;
using TP2_API.Models;
using TP2_API.Utils;

namespace TP2_API.Repository;

public class UserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Users> GetUserById(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }
    
    public async Task<IEnumerable<Users>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task AddUser(Users user)
    {
        _context.Users.Add(user);
        await SaveChangesAsync(); // Save changes here

        await CreatePostgreSQLUser(user.PostgresUsername, user.PostgresPassword);
    }

    

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.UserId == id);
    }
    
    public async Task CreatePostgreSQLUser(string username, string encryptedPassword)
    {
        // Decrypt the password
        var password = EncryptionHelper.DecryptString(encryptedPassword);

        // Now, use the decrypted password to create the PostgreSQL user
        var usernameParam = new NpgsqlParameter("username", username);
        var passwordParam = new NpgsqlParameter("password", password);
        await _context.Database.ExecuteSqlRawAsync("SELECT admin.create_restricted_user(@username, @password)", usernameParam, passwordParam);
    }
    
    
    
    public async Task<Users?> AuthenticateUser(string username, string password)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Username == username);

        if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            // User authenticated successfully
            return user;
        }

        // Authentication failed
        return null;
    }

    
    
    public async Task<bool> PromoteUserToAdmin(string username, string requesterUsername)
    {
        var requestingUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == requesterUsername);
        if (requestingUser == null || !requestingUser.IsAdmin)
        {
            return false; // Not authorized or requester not found
        }

        var userToPromote = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (userToPromote == null || userToPromote.IsAdmin)
        {
            return false; // User not found or already an admin
        }

        userToPromote.IsAdmin = true;
        await SaveChangesAsync();

        await PromotePostgreSQLUser(username); // Promote the user in PostgreSQL
        return true;
    }


     
     public async Task PromotePostgreSQLUser(string username)
     {
         var usernameParam = new NpgsqlParameter("username", username);
         await _context.Database.ExecuteSqlRawAsync("SELECT admin.promote_to_admin(@username)", usernameParam);
     }

    
     public async Task<DeleteUserResult> DeleteUser(int userId, string requesterUsername)
     {
         var requester = await _context.Users.SingleOrDefaultAsync(u => u.Username == requesterUsername);
         if (requester == null || !requester.IsAdmin)
         {
             return DeleteUserResult.NotAuthorized;
         }

         var userToDelete = await _context.Users.FindAsync(userId);
         if (userToDelete == null)
         {
             return DeleteUserResult.UserNotFound;
         }

         if (DateTime.UtcNow - userToDelete.LastLoginDate < TimeSpan.FromDays(7))
         {
             return DeleteUserResult.NotInactiveLongEnough;
         }

         _context.Users.Remove(userToDelete);
         await _context.SaveChangesAsync();

         return DeleteUserResult.Success;
     }


}
