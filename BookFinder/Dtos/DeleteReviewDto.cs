using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for deleting a review
  // Contains user and review identification for deletion authorization and processing
  public class DeleteReviewDto
  {
    // ID of the user requesting the deletion - required field
    // Used to verify ownership before deletion
    [Required]
    public int UserId { get; set; }

    // ID of the review to be deleted - required field
    [Required]
    public int ReviewId { get; set; }
  }
}
