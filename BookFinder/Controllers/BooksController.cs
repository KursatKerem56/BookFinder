using Microsoft.AspNetCore.Mvc;
using System.Data;
using BookFinder.Dtos;
using Dapper;

public class BooksController(IDbConnection db) : Controller
{
  private IDbConnection _db = db;

  public async Task<IActionResult> Index()
  {
    _db.Open();

    var books = await _db.QueryAsync("SELECT * FROM \"Books\"");

    _db.Close();

    ViewBag.Books = books;
    return View();
  }

  public async Task<IActionResult> Details(string id)
  {
    if (string.IsNullOrEmpty(id))
    {
      return BadRequest();
    }

    _db.Open();

    var book = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM findbooksdetail({id})");

    if (book == null)
    {
      _db.Close();
      return NotFound();
    }

    var foundReviews = await _db.QueryAsync($"SELECT * FROM findreviewsbybookid({id})");

    if (foundReviews != null)
    {
      foreach (var review in foundReviews)
      {
        var foundComments = await _db.QueryAsync($"SELECT * FROM findcommentsbyreviewid({review.PostId})");

        if (foundComments != null)
        {
          review.Comments = foundComments;
        }
      }

      ViewBag.Reviews = foundReviews;
    }


    _db.Close();

    ViewBag.Book = book;

    return View();
  }

  [HttpPost("books/create-review")]
  public async Task<IActionResult> Create([FromBody] CreateReviewDto model)
  {
    var UserId = model.UserId;
    var BookId = model.BookId;
    var Content = model.Content;
    var ContainsSpoilers = model.ContainsSpoilers;

    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    var foundBook = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Books\" WHERE \"bookId\" = @BookId", new { BookId });
    if (foundBook == null)
    {
      return NotFound("Book not found.");
    }

    if (string.IsNullOrWhiteSpace(Content))
    {
      return BadRequest("Content cannot be empty.");
    }

    var sql = "INSERT INTO \"Reviews\" (\"userId\", \"bookId\", \"postContent\", \"containsSpoilers\") VALUES (@UserId, @BookId, @Content, @ContainsSpoilers)";
    await _db.ExecuteAsync(sql, new { UserId, BookId, Content, ContainsSpoilers });

    return Ok($"Create review for book {model.BookId} by user {model.UserId} with content: '{model.Content}' -- spoilers: {model.ContainsSpoilers}");
  }

  [HttpPost("books/create-comment")]
  public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto model)
  {
    var UserId = model.UserId;
    var ReviewId = model.ReviewId;
    var Content = model.Content;
    var ContainsSpoilers = model.ContainsSpoilers;

    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    var foundReview = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Reviews\" WHERE \"postId\" = @ReviewId", new { ReviewId });
    if (foundReview == null)
    {
      return NotFound("Review not found.");
    }

    if (string.IsNullOrWhiteSpace(Content))
    {
      return BadRequest("Content cannot be empty.");
    }

    var sql = "INSERT INTO \"ReviewComments\" (\"userId\", \"reviewId\", \"postContent\", \"containsSpoilers\") VALUES (@UserId, @ReviewId, @Content, @ContainsSpoilers)";
    await _db.ExecuteAsync(sql, new { UserId, ReviewId, Content, ContainsSpoilers });
    return Ok($"Create comment for review {model.ReviewId} by user {model.UserId} with content: '{model.Content}' -- spoilers: {model.ContainsSpoilers}");
  }

  [HttpPost("books/delete-review")]
  public async Task<IActionResult> Delete([FromBody] DeleteReviewDto model)
  {
    var UserId = model.UserId;
    var ReviewId = model.ReviewId;

    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    var foundReview = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Reviews\" WHERE \"postId\" = @ReviewId", new { ReviewId });
    if (foundReview == null)
    {
      return NotFound("Review not found.");
    }

    if (foundReview.userId != UserId)
    {
      return Forbid("You are not authorized to delete this review.");
    }

    var sql = "DELETE FROM \"Reviews\" WHERE \"postId\" = @ReviewId";
    await _db.ExecuteAsync(sql, new { ReviewId });

    return Ok($"Deleted review with ID: {model.ReviewId}");
  }

  [HttpPost("books/delete-comment")]
  public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentDto model)
  {
    var UserId = model.UserId;
    var CommentId = model.CommentId;

    var foundUser = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"Users\" WHERE \"userId\" = @UserId", new { UserId });
    if (foundUser == null)
    {
      return NotFound("User not found.");
    }

    var foundComment = await _db.QueryFirstOrDefaultAsync("SELECT * FROM \"ReviewComments\" WHERE \"postId\" = @CommentId", new { CommentId });
    if (foundComment == null)
    {
      return NotFound("Comment not found.");
    }

    if (foundComment.userId != UserId)
    {
      return Forbid("You are not authorized to delete this comment.");
    }

    var sql = "DELETE FROM \"ReviewComments\" WHERE \"postId\" = @CommentId";
    await _db.ExecuteAsync(sql, new { CommentId });

    return Ok($"Deleted comment with ID: {model.CommentId}");
  }
}