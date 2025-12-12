using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  public class DeleteReviewDto
  {
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ReviewId { get; set; }
  }
}
