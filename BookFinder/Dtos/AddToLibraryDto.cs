using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for adding a book to a user's library
  // Contains book information and initial reading progress data
  public class AddToLibraryDto
  {
    // ID of the user adding the book to their library - required field
    [Required]
    public int UserId { get; set; }

    // ID of the book being added to the library - required field
    [Required]
    public int BookId { get; set; }

    // Current reading progress in pages - required field
    [Required]
    public int Progress { get; set; }

    // User's score/rating for the book (typically 1-5) - required field
    [Required]
    public int Score { get; set; }

    // Reading status/condition ID (e.g., reading, completed, paused) - required field
    [Required]
    public int Condition { get; set; }
  }
}
