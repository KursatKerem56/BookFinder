using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  public class AddToLibraryDto
  {
    [Required]
    public int UserId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int Progress { get; set; }

    [Required]
    public int Score { get; set; }

    [Required]
    public int Condition { get; set; }
  }
}
