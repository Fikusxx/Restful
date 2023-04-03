using AutoMapper;
using Library.API.Entities;
using Library.Models;

namespace Library.MappingProfiles;

public class CoursesProfile : Profile
{
    public CoursesProfile()
    {
        CreateMap<Course, CourseDTO>().ReverseMap();    
        CreateMap<Course, CreateCourseDTO>().ReverseMap();
    }
}
