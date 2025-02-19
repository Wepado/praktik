using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfile));
var app = builder.Build();

var message = " ����������� ������� ������, ��������� � ���������� �����.";

// *** USERS ***
app.MapGet("/users", async (ApplicationDbContext db) => await db.Users.ToListAsync());

app.MapGet("/users/{id}", async (int id, ApplicationDbContext db) =>
    await db.Users.FindAsync(id) is User user ? Results.Ok(user) : Results.NotFound());

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
});

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
});

app.MapDelete("/users/{id}", async (int id, ApplicationDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// *** PRODUCTS ***
app.MapGet("/products", async (ApplicationDbContext db) => await db.Products.ToListAsync());

app.MapGet("/products/{id}", async (int id, ApplicationDbContext db) =>
    await db.Products.FindAsync(id) is Product product ? Results.Ok(product) : Results.NotFound());

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
});

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
});

app.MapDelete("/products/{id}", async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// *** MEALS ***
app.MapGet("/meals", async (ApplicationDbContext db) => await db.Meals.ToListAsync());

app.MapPost("/meals", async (Meal meal, ApplicationDbContext db) =>
{
    if (meal.UserId <= 0 || meal.TotalCalories <= 0 || string.IsNullOrWhiteSpace(meal.MealType))
    {
        return Results.BadRequest(new { message });
    }

    db.Meals.Add(meal);
    await db.SaveChangesAsync();
    return Results.Created($"/meals/{meal.Id}", meal);
});

app.MapDelete("/meals/{id}", async (int id, ApplicationDbContext db) =>
{
    var meal = await db.Meals.FindAsync(id);
    if (meal is null) return Results.NotFound();

    db.Meals.Remove(meal);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// *** MEAL ENTRIES ***
app.MapGet("/mealentries", async (ApplicationDbContext db) => await db.MealEntries.ToListAsync());

app.MapPost("/mealentries", async (MealEntry mealEntry, ApplicationDbContext db) =>
{
    if (mealEntry.Quantity <= 0 || mealEntry.Calories <= 0)
    {
        return Results.BadRequest(new { message });
    }

    db.MealEntries.Add(mealEntry);
    await db.SaveChangesAsync();
    return Results.Created($"/mealentries/{mealEntry.Id}", mealEntry);
});

app.MapDelete("/mealentries/{id}", async (int id, ApplicationDbContext db) =>
{
    var mealEntry = await db.MealEntries.FindAsync(id);
    if (mealEntry is null) return Results.NotFound();

    db.MealEntries.Remove(mealEntry);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// *** CALORIES BURNED ***
app.MapGet("/caloriesburned", async (ApplicationDbContext db) => await db.CaloriesBurneds.ToListAsync());

app.MapPost("/caloriesburned", async (CaloriesBurned caloriesBurned, ApplicationDbContext db) =>
{
    if (caloriesBurned.BURNED <= 0)
    {
        return Results.BadRequest(new { message });
    }

    db.CaloriesBurneds.Add(caloriesBurned);
    await db.SaveChangesAsync();
    return Results.Created($"/caloriesburned/{caloriesBurned.Id}", caloriesBurned);
});

app.MapDelete("/caloriesburned/{id}", async (int id, ApplicationDbContext db) =>
{
    var caloriesBurned = await db.CaloriesBurneds.FindAsync(id);
    if (caloriesBurned is null) return Results.NotFound();

    db.CaloriesBurneds.Remove(caloriesBurned);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// *** DAILY REPORTS ***
app.MapGet("/dailyreports", async (ApplicationDbContext db) => await db.DailyReports.ToListAsync());

app.MapPost("/dailyreports", async (DailyReport dailyReport, ApplicationDbContext db) =>
{
    if (dailyReport.TotalCaloriesConsumed <= 0 || dailyReport.TotalCaloriesBurned < 0)
    {
        return Results.BadRequest(new { message });
    }

    db.DailyReports.Add(dailyReport);
    await db.SaveChangesAsync();
    return Results.Created($"/dailyreports/{dailyReport.Id}", dailyReport);
});

app.MapDelete("/dailyreports/{id}", async (int id, ApplicationDbContext db) =>
{
    var dailyReport = await db.DailyReports.FindAsync(id);
    if (dailyReport is null) return Results.NotFound();

    db.DailyReports.Remove(dailyReport);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

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

