using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  public class DeleteLibraryDto
  {
    [Required]
    public int UserId { get; set; }

    [Required]
    public int BookId { get; set; }
  }
}
