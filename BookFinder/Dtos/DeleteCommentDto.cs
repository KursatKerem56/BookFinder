using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for deleting a comment
  // Contains user and comment identification for deletion authorization and processing
  public class DeleteCommentDto
  {
    // ID of the user requesting the deletion - required field
    // Used to verify ownership before deletion
    [Required]
    public int UserId { get; set; }

    // ID of the comment to be deleted - required field
    [Required]
    public int CommentId { get; set; }
  }
}
