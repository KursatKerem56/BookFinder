using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  public class DeleteCommentDto
  {
    [Required]
    public int UserId { get; set; }

    [Required]
    public int CommentId { get; set; }
  }
}
