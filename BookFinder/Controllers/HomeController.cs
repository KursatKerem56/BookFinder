using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

public class HomeController(IDbConnection db) : Controller
{
  private IDbConnection _db = db;

  public async Task<IActionResult> Index()
  {
    _db.Open();

    var topTenBooks = await _db.QueryAsync("SELECT * FROM findtoptenbooks()");
    var books = await _db.QueryAsync("SELECT * FROM \"Books\"");

    _db.Close();

    ViewBag.TopTenBooks = topTenBooks;
    ViewBag.Books = books;

    return View();
  }

  [HttpGet("/publisher/{id}")]
  public async Task<IActionResult> Publisher(string id)
  {
    if (string.IsNullOrEmpty(id))
    {
      return BadRequest();
    }

    _db.Open();

    var publisher = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM \"Publishers\" WHERE \"publisherId\" = {id}");

    if (publisher == null)
    {
      _db.Close();
      return NotFound();
    }

    var publishedBooks = await _db.QueryAsync($"SELECT * FROM findpublishersbooks({id})");

    ViewBag.Publisher = publisher;
    ViewBag.PublishedBooks = publishedBooks;

    _db.Close();

    return View();
  }

  [HttpGet("/author/{id}")]
  public async Task<IActionResult> Author(string id)
  {
    if (string.IsNullOrEmpty(id))
    {
      return BadRequest();
    }

    _db.Open();

    var author = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM \"Authors\" WHERE \"authorId\" = {id}");

    if (author == null)
    {
      _db.Close();
      return NotFound();
    }

    var writtenBooks = await _db.QueryAsync($"SELECT * FROM findauthorsbooks({id})");

    ViewBag.Author = author;
    ViewBag.WrittenBooks = writtenBooks;

    _db.Close();

    return View();
  }
}