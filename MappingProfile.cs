using AutoMapper;
using practica.DTO;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Meal, MealDto>().ReverseMap();
        CreateMap<MealEntry, MealEntryDto>().ReverseMap();
        CreateMap<DailyReport, DailyReportDto>().ReverseMap();
    }
}