using System.Data;
using Npgsql;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDbConnection>(sp =>
{
  var config = sp.GetRequiredService<IConfiguration>();
  return new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();

app.Run();