using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Library.Models;
using Library.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/authors")]
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
	[HttpHead]
	public ActionResult<IEnumerable<AuthorDTO>> GetAuthors([FromQuery] AuthorsResourceParameters parameters)
	{
		var authors = libraryRepository.GetAuthors(parameters).ToList();
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

	[HttpPost]
	public ActionResult<AuthorDTO> CreateAuthor(CreateAuthorDTO authorDTO)
	{
		var author = mapper.Map<Author>(authorDTO);
		libraryRepository.AddAuthor(author);
		libraryRepository.Save();

		var authorToReturn = mapper.Map<AuthorDTO>(author);
		
		return CreatedAtAction(nameof(GetById), new { authorId = authorToReturn.Id }, authorToReturn);
	}
}
