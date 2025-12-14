using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for creating a comment on a review
  // Contains all necessary information to create a comment post on an existing review
  public class CreateCommentDto
  {
    // ID of the user creating the comment - required field
    [Required]
    public int UserId { get; set; }

    // ID of the review being commented on - required field
    [Required]
    public int ReviewId { get; set; }

    // The comment content/text - required field
    [Required]
    public string Content { get; set; }

    // Flag indicating whether the comment contains spoilers - required field
    [Required]
    public bool ContainsSpoilers { get; set; }
  }
}
