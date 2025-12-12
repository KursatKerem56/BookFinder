using Microsoft.AspNetCore.Mvc;
using System.Data;
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
}