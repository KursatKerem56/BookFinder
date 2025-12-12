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
}