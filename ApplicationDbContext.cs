using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MealEntry> MealEntries { get; set; }
    public DbSet<CaloriesBurned> CaloriesBurneds { get; set; }
    public DbSet<DailyReport> DailyReports { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}