namespace practica.DTO
{
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


}
