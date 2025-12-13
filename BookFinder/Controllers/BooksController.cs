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

  [HttpGet("books/library/{userId}")]
  public async Task<IActionResult> Library(string userId)
  {
    if (string.IsNullOrEmpty(userId))
    {
      return BadRequest("UserId is required.");
    }

    var foundLibrary = await _db.QueryAsync($"SELECT * FROM finduserlibrary({userId})");
    if (foundLibrary == null || !foundLibrary.Any())
    {
      return NotFound("Library not found.");
    }

    ViewBag.Library = foundLibrary;

    return View();
  }

  [HttpPost("books/library/add")]
  public async Task<IActionResult> AddToLibrary([FromBody] AddToLibraryDto model)
  {
    var UserId = model.UserId;
    var BookId = model.BookId;
    var Progress = model.Progress;
    var Score = model.Score;
    var Condition = model.Condition;

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

    var sql = $"INSERT INTO \"UserLibrary\" (\"userId\", \"bookId\", \"progressPages\", \"score\", \"statusid\") VALUES ({UserId}, {BookId}, {Progress}, {Score}, {Condition})";
    await _db.ExecuteAsync(sql);

    return Ok($"Added book {foundBook.title} to user {foundUser.username}'s library.");
  }

  [HttpPost("books/library/update")]
  public async Task<IActionResult> UpdateLibraryEntry([FromBody] UpdateLibraryDto model)
  {
    var UserId = model.UserId;
    var BookId = model.BookId;
    var Progress = model.Progress;
    var Score = model.Score;
    var Condition = model.Condition;

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

    var sql = $"UPDATE \"UserLibrary\" SET \"progressPages\" = {Progress}, \"score\" = {Score}, \"statusid\" = {Condition}, \"updateDate\" = CURRENT_TIMESTAMP WHERE \"userId\" = {UserId} AND \"bookId\" = {BookId}";
    await _db.ExecuteAsync(sql);

    return Ok($"Updated library entry for book {foundBook.title} in user {foundUser.username}'s library.");
  }

  [HttpPost("books/library/delete")]
  public async Task<IActionResult> DeleteFromLibrary([FromBody] DeleteLibraryDto model)
  {
    var UserId = model.UserId;
    var BookId = model.BookId;

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

    var sql = $"DELETE FROM \"UserLibrary\" WHERE \"userId\" = {UserId} AND \"bookId\" = {BookId}";
    await _db.ExecuteAsync(sql);

    return Ok($"Deleted book {foundBook.title} from user {foundUser.username}'s library.");
  }
}