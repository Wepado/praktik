using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Meal, MealDto>().ReverseMap();
        CreateMap<MealEntry, MealEntryDto>().ReverseMap();
        CreateMap<CaloriesBurned, CaloriesBurnedDto>().ReverseMap();
        CreateMap<DailyReport, DailyReportDto>().ReverseMap();
    }
}