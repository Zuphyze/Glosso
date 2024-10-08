using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http; 
using Glosso.Data;
using Glosso.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http; 
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Glosso.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthStateService _authStateService;
        private readonly IAntiforgery _antiforgery; // Inject antiforgery service
        private readonly HttpClient _httpClient; // Declare HttpClient

        // Constructor that accepts all dependencies
        public UserService(HttpClient httpClient, ApplicationDbContext context, AuthStateService authStateService, IAntiforgery antiforgery)
        {
            _httpClient = httpClient; // Initialize HttpClient
            _context = context; // Initialize ApplicationDbContext
            _authStateService = authStateService; // Initialize AuthStateService
            _antiforgery = antiforgery; // Initialize antiforgery service
        }

        // Method to get and store the antiforgery tokens
        public TokenResponse GetAndStoreTokens(HttpContext httpContext)
        {
            var tokens = _antiforgery.GetTokens(httpContext);
            return new TokenResponse { RequestToken = tokens.RequestToken };
        }

        public async Task<User> RegisterUser(User newUser)
        {
            // Check if a user with the same username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == newUser.Username || u.Email == newUser.Email))
            {
                return null; // Indicate registration failed due to existing user
            }

            // Hash the user's password using the plain Password property
            newUser.PasswordHash = HashPassword(newUser.Password);
            newUser.CreatedAt = DateTime.Now;
            newUser.IsActive = true;

            // Add the new user to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser; // Return the newly created user object
        }

        public async Task<bool> Login(string username, string password, string antiforgeryToken)
        {
            var loginModel = new
            {
                Username = username,
                Password = password,
                AntiforgeryToken = antiforgeryToken // Include the antiforgery token
            };

            // Send POST request to the login API endpoint
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);

            if (response.IsSuccessStatusCode)
            {
                // Handle success (e.g., store token, set auth state)
                return true;
            }
            else
            {
                // Handle login failure (e.g., invalid credentials)
                return false;
            }
        }


        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            var parts = storedPasswordHash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashEnteredPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            return hashEnteredPassword == parts[1];
        }

        public void Logout()
        {
            _authStateService.IsAuthenticated = false;
        }
    }

    // Token response model
    public class TokenResponse
    {
        public string RequestToken { get; set; }
    }
}
