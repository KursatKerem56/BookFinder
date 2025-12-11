using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using BookFinder.Dtos;

public class AuthController(IDbConnection db) : Controller
{
  private readonly IDbConnection _db = db;

  public IActionResult Index()
  {
    return View();
  }

  [HttpPost("auth/login")]
  public async Task<IActionResult> Login([FromBody] LoginDto model)
  {
    if (!ModelState.IsValid) return BadRequest(ModelState);

    // Fetch user by username
    var user = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM \"Users\" WHERE \"username\" = '{model.Username}'");

    if (user == null) return Unauthorized("Invalid username or password");

    // Verify password
    if (model.Password != user.password) return Unauthorized("Invalid username or password");

    // Build claims
    var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, user.userId.ToString()),
        new Claim(ClaimTypes.Name, user.username),
        new Claim(ClaimTypes.Role, user.role)
      };

    // Authentication identity
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Ok("Logged in");
  }
}
