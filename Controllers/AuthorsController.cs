using AutoMapper;
using Library.API.Services;
using Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
	private readonly IMapper mapper;
	private readonly ILibraryRepository libraryRepository;

	public AuthorsController(ILibraryRepository libraryRepository, IMapper mapper)
	{
		this.libraryRepository = libraryRepository;
		this.mapper = mapper;
	}

	[HttpGet]
	public ActionResult<IEnumerable<AuthorDTO>> GetAll()
	{
		var authors = libraryRepository.GetAuthors().ToList();
		var authorsDTO = mapper.Map<List<AuthorDTO>>(authors);
		
		return Ok(authorsDTO);
	}

	[HttpGet]
	[Route("{authorId}")]
	public ActionResult<AuthorDTO> GetById(Guid authorId)
	{
		var author = libraryRepository.GetAuthor(authorId);

		if (author == null)
			return NotFound(new { IsSuccess = false, Message = nameof(authorId) + " doesnt exist" });

		var authorDTO = mapper.Map<AuthorDTO>(author);

		return Ok(authorDTO);
	}
}
