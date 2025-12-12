using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  public class CreateReviewDto
  {
    [Required]
    public int UserId { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    public bool ContainsSpoilers { get; set; }
  }
}
