using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using BookFinder.Dtos;

// AuthController handles user authentication operations including login and registration
public class AuthController(IDbConnection db) : Controller
{
  // Dependency injection for database connection
  private readonly IDbConnection _db = db;

  // Display the auth index page
  public IActionResult Index()
  {
    return View();
  }

  // Display the login form view
  public IActionResult Login()
  {
    return View();
  }

  // Display the registration form view
  public IActionResult Register()
  {
    return View();
  }

  // Handle user login request with username and password validation
  [HttpPost("auth/login")]
  public async Task<IActionResult> Login([FromBody] LoginDto model)
  {
    // Validate the incoming request model
    if (!ModelState.IsValid) return BadRequest(ModelState);

    // Fetch user from database by username
    var user = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM \"Users\" WHERE \"username\" = '{model.Username}'");

    // Check if user exists
    if (user == null) return Unauthorized("Invalid username or password");

    // Verify the provided password matches the stored password
    if (model.Password != user.password) return Unauthorized("Invalid username or password");

    // Build claims containing user information for authentication
    var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, user.userId.ToString()),
        new Claim(ClaimTypes.Name, user.username),
        new Claim(ClaimTypes.Role, user.role)
      };

    // Create authentication identity and principal
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    // Sign in the user with cookie authentication
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Ok("Logged in");
  }

  // Handle user logout by clearing the authentication cookie
  [HttpPost("auth/logout")]
  public async Task<IActionResult> Logout()
  {
    // Sign out the user and clear authentication cookie
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Ok("Logged out");
  }

  // Handle user registration with validation and account creation
  [HttpPost("auth/register")]
  public async Task<IActionResult> Register([FromBody] RegisterDto model)
  {
    // Validate the incoming request model
    if (!ModelState.IsValid) return BadRequest(ModelState);

    // Validate that username does not contain spaces
    if (model.Username.Contains(' '))
      return BadRequest("Username cannot contain spaces.");

    // Check if username already exists in the database
    var existingUser = await _db.QueryFirstOrDefaultAsync($"SELECT * FROM \"Users\" WHERE \"username\" = '{model.Username}'");
    if (existingUser != null) return Conflict("Username already exists");

    // Insert new user into the database with default "user" role
    var insertQuery = "INSERT INTO \"Users\" (\"username\", \"password\", \"role\") VALUES (@Username, @Password, @Role)";
    await _db.ExecuteAsync(insertQuery, new { Username = model.Username, Password = model.Password, Role = "user" });

    return Ok("User registered");
  }
}
