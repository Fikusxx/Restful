using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Library.Helpers;
using Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/authorcollections")]
public class AuthorCollectionsController : ControllerBase
{
	private readonly IMapper mapper;
	private readonly ILibraryRepository libraryRepository;

	public AuthorCollectionsController(ILibraryRepository libraryRepository, IMapper mapper)
	{
		this.libraryRepository = libraryRepository;
		this.mapper = mapper;
	}

	[HttpGet]
	[Route("({ids})")]
	public IActionResult GetAuthorCollection(
		[FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
	{
		if (ids == null)
			return BadRequest();

		var authorEntities = libraryRepository.GetAuthors(ids);

		if (ids.Count() != authorEntities.Count())
			return NotFound();

		var authors = mapper.Map<IEnumerable<AuthorDTO>>(authorEntities);

		return Ok(authors);
	}

	[HttpPost]
	public ActionResult<IEnumerable<AuthorDTO>> CreateAuthorCollection(IEnumerable<CreateAuthorDTO> authorCollection)
	{
		var authorEntities = mapper.Map<List<Author>>(authorCollection);
		authorEntities.ForEach(author => libraryRepository.AddAuthor(author));
		libraryRepository.Save();

		var authorsCollection = mapper.Map<IEnumerable<AuthorDTO>>(authorEntities);
		var idsAsString = string.Join(",", authorsCollection.Select(x => x.Id));

		return CreatedAtAction(nameof(GetAuthorCollection), new { ids = idsAsString }, authorsCollection);
	}
}
