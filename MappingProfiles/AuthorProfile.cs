using AutoMapper;
using Library.API.Entities;
using Library.Helpers;
using Library.Models;

namespace Library.MappingProfiles;

public class AuthorProfile : Profile
{
	public AuthorProfile()
	{
		CreateMap<Author, AuthorDTO>()
			.ForMember(authorDTO => authorDTO.Name,
				options => options.MapFrom(author => $"{author.FirstName} {author.LastName}"))
			.ForMember(authorDTO => authorDTO.Age,
				options => options.MapFrom(author => author.DateOfBirth.GetCurrentAge()))
			.ReverseMap();

		CreateMap<Author, CreateAuthorDTO>().ReverseMap();
	}
}
