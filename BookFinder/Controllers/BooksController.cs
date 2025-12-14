using Microsoft.AspNetCore.Mvc;
using System.Data;
using BookFinder.Dtos;
using Dapper;

// BooksController handles all book-related operations including viewing books, managing reviews and comments, and managing user libraries
public class BooksController(IDbConnection db) : Controller
{
  // Dependency injection for database connection
  private IDbConnection _db = db;

  // Retrieve and display all books from the database
  public async Task<IActionResult> Index()
  {
    // Open database connection
    _db.Open();

    // Query all books from the Books table
    var books = await _db.QueryAsync("SELECT * FROM \"Books\"");

    // Close database connection
    _db.Close();

    // Pass books to the view
    ViewBag.Books = books;
    return View();
  }

  // Retrieve detailed information about a specific book including reviews and comments
  public async Task<IActionResult> Details(string id)
  {
    // Validate that book ID is provided
    if (string.IsNullOrEmpty(id))
    {
      return BadRequest();
    }

    // Open database connection
    _db.Open();

    // Query book details using database function
    var book = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM findbooksdetail({id})");

    // Check if book exists
    if (book == null)
    {
      _db.Close();
      return NotFound();
    }

    // Query all reviews for the book
    var foundReviews = await _db.QueryAsync($"SELECT * FROM findreviewsbybookid({id})");

    // If reviews exist, fetch associated comments for each review
    if (foundReviews != null)
    {
      foreach (var review in foundReviews)
      {
        var foundComments = await _db.QueryAsync($"SELECT * FROM findcommentsbyreviewid({review.PostId})");

        // Add comments to the review object
        if (foundComments != null)
        {
          review.Comments = foundComments;
        }
      }

      // Pass reviews to the view
      ViewBag.Reviews = foundReviews;
    }

    // Close database connection
    _db.Close();

    // Pass book details to the view
    ViewBag.Book = book;

    return View();
  }

  // Create a new review for a book
  [HttpPost("books/create-review")]
  public async Task<IActionResult> Create([FromBody] CreateReviewDto model)
  {
    // Extract review details from the DTO
    var UserId = model.UserId;
    var BookId = model.BookId;
    var Content = model.Content;
    var ContainsSpoilers = model.ContainsSpoilers;

    // Verify that the user exists in the database
    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    // Verify that the book exists in the database
    var foundBook = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Books\" WHERE \"bookId\" = @BookId", new { BookId });
    if (foundBook == null)
    {
      return NotFound("Book not found.");
    }

    // Validate that review content is not empty
    if (string.IsNullOrWhiteSpace(Content))
    {
      return BadRequest("Content cannot be empty.");
    }

    // Insert the new review into the database
    var sql = "INSERT INTO \"Reviews\" (\"userId\", \"bookId\", \"postContent\", \"containsSpoilers\") VALUES (@UserId, @BookId, @Content, @ContainsSpoilers)";
    await _db.ExecuteAsync(sql, new { UserId, BookId, Content, ContainsSpoilers });

    return Ok($"Create review for book {model.BookId} by user {model.UserId} with content: '{model.Content}' -- spoilers: {model.ContainsSpoilers}");
  }

  // Create a new comment on an existing review
  [HttpPost("books/create-comment")]
  public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto model)
  {
    // Extract comment details from the DTO
    var UserId = model.UserId;
    var ReviewId = model.ReviewId;
    var Content = model.Content;
    var ContainsSpoilers = model.ContainsSpoilers;

    // Verify that the user exists in the database
    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    // Verify that the review exists in the database
    var foundReview = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Reviews\" WHERE \"postId\" = @ReviewId", new { ReviewId });
    if (foundReview == null)
    {
      return NotFound("Review not found.");
    }

    // Validate that comment content is not empty
    if (string.IsNullOrWhiteSpace(Content))
    {
      return BadRequest("Content cannot be empty.");
    }

    // Insert the new comment into the database
    var sql = "INSERT INTO \"ReviewComments\" (\"userId\", \"reviewId\", \"postContent\", \"containsSpoilers\") VALUES (@UserId, @ReviewId, @Content, @ContainsSpoilers)";
    await _db.ExecuteAsync(sql, new { UserId, ReviewId, Content, ContainsSpoilers });
    return Ok($"Create comment for review {model.ReviewId} by user {model.UserId} with content: '{model.Content}' -- spoilers: {model.ContainsSpoilers}");
  }

  // Delete a review (only the review owner can delete)
  [HttpPost("books/delete-review")]
  public async Task<IActionResult> Delete([FromBody] DeleteReviewDto model)
  {
    // Extract deletion details from the DTO
    var UserId = model.UserId;
    var ReviewId = model.ReviewId;

    // Verify that the user exists in the database
    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    // Verify that the review exists in the database
    var foundReview = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Reviews\" WHERE \"postId\" = @ReviewId", new { ReviewId });
    if (foundReview == null)
    {
      return NotFound("Review not found.");
    }

    // Check if the current user is the owner of the review
    if (foundReview.userId != UserId)
    {
      return Forbid("You are not authorized to delete this review.");
    }

    // Delete the review from the database
    var sql = "DELETE FROM \"Reviews\" WHERE \"postId\" = @ReviewId";
    await _db.ExecuteAsync(sql, new { ReviewId });

    return Ok($"Deleted review with ID: {model.ReviewId}");
  }

  // Delete a comment (only the comment owner can delete)
  [HttpPost("books/delete-comment")]
  public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentDto model)
  {
    // Extract deletion details from the DTO
    var UserId = model.UserId;
    var CommentId = model.CommentId;

    // Verify that the user exists in the database
    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    // Verify that the comment exists in the database
    var foundComment = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"ReviewComments\" WHERE \"postId\" = @CommentId", new { CommentId });
    if (foundComment == null)
    {
      return NotFound("Comment not found.");
    }

    // Check if the current user is the owner of the comment
    if (foundComment.userId != UserId)
    {
      return Forbid("You are not authorized to delete this comment.");
    }

    // Delete the comment from the database
    var sql = "DELETE FROM \"ReviewComments\" WHERE \"postId\" = @CommentId";
    await _db.ExecuteAsync(sql, new { CommentId });

    return Ok($"Deleted comment with ID: {model.CommentId}");
  }

  // Retrieve a user's library containing their books and reading progress
  [HttpGet("books/library/{userId}")]
  public async Task<IActionResult> Library(string userId)
  {
    // Validate that user ID is provided
    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("UserId is required.");
    }

    // Query the user's library using database function
    var foundLibrary = await _db.QueryAsync($"SELECT * FROM finduserlibrary({userId})");

    // Check if library exists and contains entries
    if (foundLibrary == null || !foundLibrary.Any())
    {
      return NotFound("Library not found.");
    }

    // Pass library to the view
    ViewBag.Library = foundLibrary;

    return View();
  }

  // Add a book to a user's library with initial reading progress and score
  [HttpPost("books/library/add")]
  public async Task<IActionResult> AddToLibrary([FromBody] AddToLibraryDto model)
  {
    // Extract library entry details from the DTO
    var UserId = model.UserId;
    var BookId = model.BookId;
    var Progress = model.Progress;
    var Score = model.Score;
    var Condition = model.Condition;

    // Verify that the user exists in the database
    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    // Verify that the book exists in the database
    var foundBook = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Books\" WHERE \"bookId\" = @BookId", new { BookId });
    if (foundBook == null)
    {
      return NotFound("Book not found.");
    }

    // Insert new library entry into the database
    var sql = $"INSERT INTO \"UserLibrary\" (\"userId\", \"bookId\", \"progressPages\", \"score\", \"statusId\") VALUES ({UserId}, {BookId}, {Progress}, {Score}, {Condition})";
    await _db.ExecuteAsync(sql);

    return Ok($"Added book {foundBook.title} to user {foundUser.username}'s library.");
  }

  // Update an existing library entry with new progress, score, and reading status
  [HttpPost("books/library/update")]
  public async Task<IActionResult> UpdateLibraryEntry([FromBody] UpdateLibraryDto model)
  {
    // Extract updated library entry details from the DTO
    var UserId = model.UserId;
    var BookId = model.BookId;
    var Progress = model.Progress;
    var Score = model.Score;
    var Condition = model.Condition;

    // Verify that the user exists in the database
    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    // Verify that the book exists in the database
    var foundBook = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Books\" WHERE \"bookId\" = @BookId", new { BookId });
    if (foundBook == null)
    {
      return NotFound("Book not found.");
    }

    // Update the library entry with new values and current timestamp
    var sql = $"UPDATE \"UserLibrary\" SET \"progressPages\" = {Progress}, \"score\" = {Score}, \"statusid\" = {Condition}, \"updateDate\" = CURRENT_TIMESTAMP WHERE \"userId\" = {UserId} AND \"bookId\" = {BookId}";
    await _db.ExecuteAsync(sql);

    return Ok($"Updated library entry for book {foundBook.title} in user {foundUser.username}'s library.");
  }

  // Delete a book from a user's library
  [HttpPost("books/library/delete")]
  public async Task<IActionResult> DeleteFromLibrary([FromBody] DeleteLibraryDto model)
  {
    // Extract deletion details from the DTO
    var UserId = model.UserId;
    var BookId = model.BookId;

    // Verify that the user exists in the database
    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    // Verify that the book exists in the database
    var foundBook = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Books\" WHERE \"bookId\" = @BookId", new { BookId });
    if (foundBook == null)
    {
      return NotFound("Book not found.");
    }

    // Delete the library entry from the database
    var sql = $"DELETE FROM \"UserLibrary\" WHERE \"userId\" = {UserId} AND \"bookId\" = {BookId}";
    await _db.ExecuteAsync(sql);

    return Ok($"Deleted book {foundBook.title} from user {foundUser.username}'s library.");
  }
}