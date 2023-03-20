using Library.API.Services;
using Library.Helpers;
using Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
	private readonly ILibraryRepository libraryRepository;

	public AuthorsController(ILibraryRepository libraryRepository)
	{
		this.libraryRepository = libraryRepository;
	}

	[HttpGet]
	public IActionResult GetAll()
	{
		var authors = libraryRepository.GetAuthors().ToList();
		var authorsDTO = new List<AuthorDTO>();
		authors.ForEach(x => authorsDTO.Add(new AuthorDTO
		{
			Id = x.Id,
			Name = $"{x.FirstName} {x.LastName}",
			Age = x.DateOfBirth.GetCurrentAge(),
			MainCategory = x.MainCategory
		}));


		return Ok(authorsDTO);
	}

	[HttpGet]
	[Route("{authorId}")]
	public IActionResult GetById(Guid authorId)
	{
		var author = libraryRepository.GetAuthor(authorId);

		if (author == null)
			return NotFound(new { IsSuccess = false, Message = nameof(authorId) + " doesnt exist" });

		return Ok(author);
	}
}
