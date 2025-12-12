using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  public class CreateCommentDto
  {
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ReviewId { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    public bool ContainsSpoilers { get; set; }
  }
}
