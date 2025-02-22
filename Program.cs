using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // ��������� ������ � ������ ������
              .AllowAnyMethod()  // ��������� ����� HTTP-����� (GET, POST � �.�.)
              .AllowAnyHeader(); // ��������� ����� ���������
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfile));

var key = "����������������123"; // ��������� ���� ��� ������� ������ (����� ������� � `appsettings.json`)

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();

var message = " ����������� ������� ������, ��������� � ���������� �����.";

app.UseCors("AllowAll"); // ��������� CORS ��������

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/register", async (User user, ApplicationDbContext db) =>
{
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
    if (existingUser != null)
        return Results.BadRequest("Email ��� ������������!");

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok("������������ ���������������!");
});

app.MapPost("/login", async (UserLoginDto loginDto, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);
    if (user == null)
        return Results.BadRequest("�������� email ��� ������!");

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    };

    var keyBytes = Encoding.UTF8.GetBytes(key);
    var keySymmetric = new SymmetricSecurityKey(keyBytes);
    var creds = new SigningCredentials(keySymmetric, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { Token = tokenString });
});

// *** USERS ***
app.MapGet("/users", async (ApplicationDbContext db) => await db.Users.ToListAsync())
    .RequireAuthorization(); // ��������

app.MapGet("/users/{id}", async (int id, ApplicationDbContext db) =>
    await db.Users.FindAsync(id) is User user ? Results.Ok(user) : Results.NotFound())
    .RequireAuthorization(); // ��������

app.MapPost("/users", async (User user, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(user.Username) || user.Username.Length < 3 ||
        string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@") ||
        user.Age < 1 || user.Age > 120 ||
        user.Weight < 1 || user.Weight > 500 ||
        user.Height < 30 || user.Height > 250)
    {
        return Results.BadRequest(new { message });
    }

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
}).RequireAuthorization(); // ��������

app.MapPut("/users/{id}", async (int id, User updatedUser, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Username = updatedUser.Username;
    user.Email = updatedUser.Email;
    user.Password = updatedUser.Password;
    user.Age = updatedUser.Age;
    user.Weight = updatedUser.Weight;
    user.Height = updatedUser.Height;
    user.Gender = updatedUser.Gender;
    user.ActivityLevel = updatedUser.ActivityLevel;
    user.DailyCalorieGoal = updatedUser.DailyCalorieGoal;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

app.MapDelete("/users/{id}", async (int id, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

// *** PRODUCTS ***
app.MapGet("/products", async (ApplicationDbContext db) => await db.Products.ToListAsync())
    .RequireAuthorization(); // ��������

app.MapGet("/products/{id}", async (int id, ApplicationDbContext db) =>
    await db.Products.FindAsync(id) is Product product ? Results.Ok(product) : Results.NotFound())
    .RequireAuthorization(); // ��������

app.MapPost("/products", async (Product product, ApplicationDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(product.Name) || product.Calories <= 0 ||
        product.Proteins < 0 || product.Fats < 0 || product.Carbohydrates < 0)
    {
        return Results.BadRequest(new { message });
    }

    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
}).RequireAuthorization(); // ��������

app.MapPut("/products/{id}", async (int id, Product updatedProduct, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    product.Name = updatedProduct.Name;
    product.Calories = updatedProduct.Calories;
    product.Proteins = updatedProduct.Proteins;
    product.Fats = updatedProduct.Fats;
    product.Carbohydrates = updatedProduct.Carbohydrates;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

app.MapDelete("/products/{id}", async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

// *** MEALS ***
app.MapGet("/meals", async (ApplicationDbContext db) => await db.Meals.ToListAsync())
    .RequireAuthorization(); // ��������

app.MapPost("/meals", async (Meal meal, ApplicationDbContext db) =>
{
    if (meal.UserId <= 0 || meal.TotalCalories <= 0 || string.IsNullOrWhiteSpace(meal.MealType))
    {
        return Results.BadRequest(new { message });
    }

    db.Meals.Add(meal);
    await db.SaveChangesAsync();
    return Results.Created($"/meals/{meal.Id}", meal);
}).RequireAuthorization(); // ��������

app.MapDelete("/meals/{id}", async (int id, ApplicationDbContext db) =>
{
    var meal = await db.Meals.FindAsync(id);
    if (meal is null) return Results.NotFound();

    db.Meals.Remove(meal);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

// *** MEAL ENTRIES ***
app.MapGet("/mealentries", async (ApplicationDbContext db) => await db.MealEntries.ToListAsync())
    .RequireAuthorization(); // ��������

app.MapPost("/mealentries", async (MealEntry mealEntry, ApplicationDbContext db) =>
{
    if (mealEntry.Quantity <= 0 || mealEntry.Calories <= 0)
    {
        return Results.BadRequest(new { message });
    }

    db.MealEntries.Add(mealEntry);
    await db.SaveChangesAsync();
    return Results.Created($"/mealentries/{mealEntry.Id}", mealEntry);
}).RequireAuthorization(); // ��������

app.MapDelete("/mealentries/{id}", async (int id, ApplicationDbContext db) =>
{
    var mealEntry = await db.MealEntries.FindAsync(id);
    if (mealEntry is null) return Results.NotFound();

    db.MealEntries.Remove(mealEntry);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

// *** CALORIES BURNED ***
app.MapGet("/caloriesburned", async (ApplicationDbContext db) => await db.CaloriesBurneds.ToListAsync())
    .RequireAuthorization(); // ��������

app.MapPost("/caloriesburned", async (CaloriesBurned caloriesBurned, ApplicationDbContext db) =>
{
    if (caloriesBurned.BURNED <= 0)
    {
        return Results.BadRequest(new { message });
    }

    db.CaloriesBurneds.Add(caloriesBurned);
    await db.SaveChangesAsync();
    return Results.Created($"/caloriesburned/{caloriesBurned.Id}", caloriesBurned);
}).RequireAuthorization(); // ��������

app.MapDelete("/caloriesburned/{id}", async (int id, ApplicationDbContext db) =>
{
    var caloriesBurned = await db.CaloriesBurneds.FindAsync(id);
    if (caloriesBurned is null) return Results.NotFound();

    db.CaloriesBurneds.Remove(caloriesBurned);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

// *** DAILY REPORTS ***
app.MapGet("/dailyreports", async (ApplicationDbContext db) => await db.DailyReports.ToListAsync())
    .RequireAuthorization(); // ��������

app.MapPost("/dailyreports", async (DailyReport dailyReport, ApplicationDbContext db) =>
{
    if (dailyReport.TotalCaloriesConsumed <= 0 || dailyReport.TotalCaloriesBurned < 0)
    {
        return Results.BadRequest(new { message });
    }

    db.DailyReports.Add(dailyReport);
    await db.SaveChangesAsync();
    return Results.Created($"/dailyreports/{dailyReport.Id}", dailyReport);
}).RequireAuthorization(); // ��������

app.MapDelete("/dailyreports/{id}", async (int id, ApplicationDbContext db) =>
{
    var dailyReport = await db.DailyReports.FindAsync(id);
    if (dailyReport is null) return Results.NotFound();

    db.DailyReports.Remove(dailyReport);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // ��������

app.Run();

// Models
public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public string Gender { get; set; }
        public string ActivityLevel { get; set; }
        public double DailyCalorieGoal { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
    }

    public class Meal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string MealType { get; set; }
        public double TotalCalories { get; set; }
    }

    public class MealEntry
    {
        public int Id { get; set; }
        public int MealId { get; set; }
        public int ProductId { get; set; }
        public double Quantity { get; set; }
        public double Calories { get; set; }
    }

    public class CaloriesBurned
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double BURNED { get; set; }
    }

    public class DailyReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double TotalCaloriesConsumed { get; set; }
        public double TotalCaloriesBurned { get; set; }
        public double CalorieBalance { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public string Gender { get; set; }
        public string ActivityLevel { get; set; }
        public double DailyCalorieGoal { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
    }

    public class MealDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string MealType { get; set; }
        public double TotalCalories { get; set; }
    }

    public class MealEntryDto
    {
        public int Id { get; set; }
        public int MealId { get; set; }
        public int ProductId { get; set; }
        public double Quantity { get; set; }
        public double Calories { get; set; }
    }

    public class CaloriesBurnedDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double Burned { get; set; }
    }

    public class DailyReportDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public double TotalCaloriesConsumed { get; set; }
        public double TotalCaloriesBurned { get; set; }
        public double CalorieBalance { get; set; }
    }

