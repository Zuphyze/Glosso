using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Glosso.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        // This property will store the hashed password
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        // This property is only for registration, and will not be mapped to the database
        [NotMapped]
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
