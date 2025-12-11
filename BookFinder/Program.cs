using System.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IDbConnection>(sp =>
{
  var config = sp.GetRequiredService<IConfiguration>();
  return new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.MapDefaultControllerRoute();

app.Run();