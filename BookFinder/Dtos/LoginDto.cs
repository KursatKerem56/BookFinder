using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for user login requests
  // Contains username and password credentials for authentication
  public class LoginDto
  {
    // Username for login - required and cannot contain spaces
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces.")]
    public string Username { get; set; }

    // Password for login - required field
    [Required]
    public string Password { get; set; }
  }
}
