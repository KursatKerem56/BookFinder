using System.ComponentModel.DataAnnotations;
namespace BookFinder.Dtos

{
  // Data Transfer Object for creating a book review
  // Contains all necessary information to create a review post for a book
  public class CreateReviewDto
  {
    // ID of the user creating the review - required field
    [Required]
    public int UserId { get; set; }

    // ID of the book being reviewed - required field
    [Required]
    public int BookId { get; set; }

    // The review content/text - required field
    [Required]
    public string Content { get; set; }

    // Flag indicating whether the review contains spoilers - required field
    [Required]
    public bool ContainsSpoilers { get; set; }
  }
}
