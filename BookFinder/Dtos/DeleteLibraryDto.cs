using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for deleting a book from a user's library
  // Contains user and book identification for deletion processing
  public class DeleteLibraryDto
  {
    // ID of the user deleting the book from their library - required field
    [Required]
    public int UserId { get; set; }

    // ID of the book being deleted from the library - required field
    [Required]
    public int BookId { get; set; }
  }
}
