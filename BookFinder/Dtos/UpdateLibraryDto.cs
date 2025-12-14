using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for updating a book in a user's library
  // Contains updated reading progress and status information
  public class UpdateLibraryDto
  {
    // ID of the user updating their library entry - required field
    [Required]
    public int UserId { get; set; }

    // ID of the book being updated in the library - required field
    [Required]
    public int BookId { get; set; }

    // Updated reading progress in pages - required field
    [Required]
    public int Progress { get; set; }

    // Updated user's score/rating for the book (typically 1-5) - required field
    [Required]
    public int Score { get; set; }

    // Updated reading status/condition ID (e.g., reading, completed, paused) - required field
    [Required]
    public int Condition { get; set; }
  }
}
