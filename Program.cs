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
        policy.AllowAnyOrigin()  // Разрешить доступ с любого домена
              .AllowAnyMethod()  // Разрешить любой HTTP-метод (GET, POST и т.д.)
              .AllowAnyHeader(); // Разрешить любые заголовки
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfile));

var key = "Я люблю своего преподователя"; // Секретный ключ для подписи токена (лучше хранить в appsettings.json)

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

var message = " Неправильно введены данные, проверьте и попробуйте снова.";

app.UseCors("AllowAll"); // Применяем CORS политику

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/register", async (User user, ApplicationDbContext db) =>
{
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
    if (existingUser != null)
        return Results.BadRequest("Email уже используется!");

    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Возвращаем Id, Username и Token (если ты его добавляешь на сервере)
    return Results.Ok(new
    {
        userId = user.Id,  // передаем Id
        username = user.Username,  // передаем username
    });
});

app.MapPost("/login", async (UserLoginDto loginDto, ApplicationDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);
    if (user == null)
        return Results.BadRequest("Неверный email или пароль!");

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
    .RequireAuthorization(); // защищаем

app.MapGet("/users/{id}", async (int id, ApplicationDbContext db) =>
    await db.Users.FindAsync(id) is User user ? Results.Ok(user) : Results.NotFound())
    .RequireAuthorization(); // защищаем

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
}).RequireAuthorization(); // защищаем

app.MapPut("/users/{id}", async (int id, User updatedUser, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    // Обновляем только те поля, которые пришли в запросе
    if (updatedUser.Username != null) user.Username = updatedUser.Username;
    if (updatedUser.Email != null) user.Email = updatedUser.Email;
    if (updatedUser.Password != null) user.Password = updatedUser.Password;
    if (updatedUser.Age > 0) user.Age = updatedUser.Age;
    if (updatedUser.Weight > 0) user.Weight = updatedUser.Weight;
    if (updatedUser.Height > 0) user.Height = updatedUser.Height;
    if (updatedUser.Gender != null) user.Gender = updatedUser.Gender;
    if (updatedUser.ActivityLevel != null) user.ActivityLevel = updatedUser.ActivityLevel;
    if (updatedUser.DailyCalorieGoal > 0) user.DailyCalorieGoal = updatedUser.DailyCalorieGoal;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/users/{id}", async (int id, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // защищаем

// *** PRODUCTS ***
app.MapGet("/products", async (ApplicationDbContext db) => await db.Products.ToListAsync())
    .RequireAuthorization(); // защищаем

app.MapGet("/products/{id}", async (int id, ApplicationDbContext db) =>
    await db.Products.FindAsync(id) is Product product ? Results.Ok(product) : Results.NotFound())
    .RequireAuthorization(); // защищаем

app.MapPost("/products", async (Product product, ClaimsPrincipal user, ApplicationDbContext db) =>
{
    // Получаем Claim с идентификатором пользователя
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }
    int userId = int.Parse(userIdClaim.Value);
    product.UserId = userId;  // Привязываем продукт к текущему пользователю

    // Проверяем входные данные
    if (string.IsNullOrWhiteSpace(product.Name) || product.Calories <= 0 ||
        product.Proteins < 0 || product.Fats < 0 || product.Carbohydrates < 0)
    {
        return Results.BadRequest(new { message = "Некорректные данные" });
    }

    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
}).RequireAuthorization();
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
}).RequireAuthorization(); // защищаем

app.MapDelete("/products/{id}", async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // защищаем



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
    public List<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Calories { get; set; }
    public double Proteins { get; set; }
    public double Fats { get; set; }
    public double Carbohydrates { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}


