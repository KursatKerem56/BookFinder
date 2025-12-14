using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for user registration requests
  // Contains username and password credentials for creating a new account
  public class RegisterDto
  {
    // Username for new account - required and cannot contain spaces
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces.")]
    public string Username { get; set; }

    // Password for new account - required field
    [Required]
    public string Password { get; set; }
  }
}
