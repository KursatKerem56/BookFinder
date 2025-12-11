using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  public class RegisterDto
  {
    [Required]
    [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces.")]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
  }
}
