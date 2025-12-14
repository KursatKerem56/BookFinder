using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

// HomeController handles home page and content discovery features including top books, author profiles, and publisher information
public class HomeController(IDbConnection db) : Controller
{
  // Dependency injection for database connection
  private IDbConnection _db = db;

  // Display the home page with top ten books and all books available
  public async Task<IActionResult> Index()
  {
    // Open database connection
    _db.Open();

    // Query the top ten books using database function
    var topTenBooks = await _db.QueryAsync("SELECT * FROM findtoptenbooks()");

    // Query all books from the Books table
    var books = await _db.QueryAsync("SELECT * FROM \"Books\"");

    // Close database connection
    _db.Close();

    // Pass top ten books to the view
    ViewBag.TopTenBooks = topTenBooks;

    // Pass all books to the view
    ViewBag.Books = books;

    return View();
  }

  // Display publisher details and all books published by that publisher
  [HttpGet("/publisher/{id}")]
  public async Task<IActionResult> Publisher(string id)
  {
    // Validate that publisher ID is provided
    if (string.IsNullOrEmpty(id))
    {
      return BadRequest();
    }

    // Open database connection
    _db.Open();

    // Query publisher details by ID
    var publisher = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM \"Publishers\" WHERE \"publisherId\" = {id}");

    // Check if publisher exists
    if (publisher == null)
    {
      _db.Close();
      return NotFound();
    }

    // Query all books published by this publisher using database function
    var publishedBooks = await _db.QueryAsync($"SELECT * FROM findpublishersbooks({id})");

    // Pass publisher details to the view
    ViewBag.Publisher = publisher;

    // Pass published books to the view
    ViewBag.PublishedBooks = publishedBooks;

    // Close database connection
    _db.Close();

    return View();
  }

  // Display author details and all books written by that author
  [HttpGet("/author/{id}")]
  public async Task<IActionResult> Author(string id)
  {
    // Validate that author ID is provided
    if (string.IsNullOrEmpty(id))
    {
      return BadRequest();
    }

    // Open database connection
    _db.Open();

    // Query author details by ID
    var author = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM \"Authors\" WHERE \"authorId\" = {id}");

    // Check if author exists
    if (author == null)
    {
      _db.Close();
      return NotFound();
    }

    // Query all books written by this author using database function
    var writtenBooks = await _db.QueryAsync($"SELECT * FROM findauthorsbooks({id})");

    // Pass author details to the view
    ViewBag.Author = author;

    // Pass written books to the view
    ViewBag.WrittenBooks = writtenBooks;

    // Close database connection
    _db.Close();

    return View();
  }
}