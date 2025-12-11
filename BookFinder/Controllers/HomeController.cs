using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

public class HomeController(IDbConnection db) : Controller
{
  private IDbConnection _db = db;

  public async Task<IActionResult> Index()
  {
    _db.Open();

    var result = await _db.QueryAsync("SELECT * FROM \"Users\"");

    _db.Close();

    ViewBag.Message = "Welcome to BookFinder! (Home)";
    ViewBag.Result = result;

    return View();
  }
}