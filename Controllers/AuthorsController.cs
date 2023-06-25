using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Library.Background;
using Library.Caching;
using Library.Filters;
using Library.Helpers;
using Library.Models;
using Library.Resources;
using Library.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Library.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
	private readonly IMapper mapper;
	private readonly ILibraryRepository libraryRepository;
	private readonly IPropertyMappingService propertyMappingService;
	private readonly IPropertyCheckerService propertyCheckerService;
	private readonly ICacheService cacheService;
	private static List<Person> persons = new();
	private readonly IMediator mediator;

	public AuthorsController(ILibraryRepository libraryRepository, IMapper mapper
		, IPropertyMappingService propertyMappingService, IPropertyCheckerService propertyCheckerService,
		ICacheService cacheService,
		IMediator mediator)
	{
		this.libraryRepository = libraryRepository;
		this.mapper = mapper;
		this.propertyMappingService = propertyMappingService;
		this.propertyCheckerService = propertyCheckerService;
		this.cacheService = cacheService;
		this.mediator = mediator;
		persons.Add(new Person() { Id = 1, Name = "Fikus", Age = 25, LastModified = new DateTime(ticks: 638232661032996201) });
	}

	[HttpGet]
	[ETagFilter]
	[Route("etag")]
	public IActionResult Etag()
	{
		var person = persons.FirstOrDefault();

		//return Ok();
		//return Ok(new Author());
		return Ok(persons);
	}

	[HttpGet]
	[Route("caching-test")]
	public async Task<IActionResult> CachingTest()
	{
		var query = new GetAuthorsQuery() { Id = Guid.NewGuid()};
		var data = await mediator.Send(query);

		return Ok(data);
	}

	[HttpGet]
	[Route("cache")]
	public IActionResult Get()
	{
		//var kekw = test.GetData(new GetAuthorsQuery() { Id = Guid.NewGuid() });
		var data = cacheService.GetData<Author>("author#1");

		if (data != null)
			return Ok(data);

		data = new Author { Id = Guid.NewGuid(), FirstName = "Fikus", LastName = "Garrosh", DateOfBirth = DateTimeOffset.UtcNow, MainCategory = "IT" };
		cacheService.SetData<Author>("author#1", data, DateTimeOffset.UtcNow.AddMinutes(10));

		return Ok(data);

		//if (data == null)
		//{
		//	var author = new Author() { FirstName = "Fikus", LastName = "Skilled", DateOfBirth = DateTimeOffset.UtcNow, MainCategory = "IT" };
		//	//var rnd = new { FirstName = "Fikus", LastName = "Skilled", DateOfBirth = DateTimeOffset.UtcNow, MainCategory = new List<int>() };
		//	cacheService.SetData<Author>("author#1", author, DateTimeOffset.UtcNow.AddMinutes(10));
		//	return Ok();
		//}

		//return Ok(data);
	}

	[HttpGet]
	[Route("cache-all")]
	public IActionResult GetAll()
	{
		var data = cacheService.GetData<IEnumerable<Author>>("authors");

		if (data != null && data.Any())
			return Ok(data);

		data = libraryRepository.GetAuthors().ToList();
		cacheService.SetData<IEnumerable<Author>>("authors", data, DateTimeOffset.UtcNow.AddMinutes(10));

		return Ok(data);
	}


	[HttpGet]
	[HttpHead]
	public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters parameters)
	{
		if (propertyMappingService.ValidMappingExistsFor<AuthorDTO, Author>(parameters.OrderBy) == false)
			return BadRequest();

		if (propertyCheckerService.TypeHasProperties<AuthorDTO>(parameters.Fields) == false)
			return BadRequest();

		var authors = libraryRepository.GetAuthors(parameters);

		var paginationMetaData = new
		{
			totalItemsCount = authors.TotalCount,
			pageSize = authors.PageSize,
			currentPage = authors.CurrentPage,
			totalPages = authors.TotalPages,
			previousPage = authors.HasPrevious ? CreateAuthorsResourceUri(parameters, ResourceUriType.PreviousPage) : null,
			nextPage = authors.HasNext ? CreateAuthorsResourceUri(parameters, ResourceUriType.NextPage) : null
		};
		HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetaData));
		var authorsDTO = mapper.Map<List<AuthorDTO>>(authors).ShapeData<AuthorDTO>(parameters.Fields);

		return Ok(authorsDTO);
	}

	[HttpGet]
	[Route("{authorId}")]
	public IActionResult GetById(Guid authorId, string? fields)
	{
		if (propertyCheckerService.TypeHasProperties<AuthorDTO>(fields) == false)
			return BadRequest();

		var author = libraryRepository.GetAuthor(authorId);

		if (author == null)
			return NotFound(new { IsSuccess = false, Message = nameof(authorId) + " doesnt exist" });

		var links = CreateLinksForAuthor(authorId, fields);
		var authorDTO = mapper.Map<AuthorDTO>(author).ShapeData(fields);
		authorDTO.TryAdd("links", links);

		return Ok(authorDTO);
	}

	[HttpPost]
	[Route("save-www")]
	public ActionResult<AuthorDTO> CreateAuthor(CreateAuthorDTO authorDTO)
	{
		var author = mapper.Map<Author>(authorDTO);
		libraryRepository.AddAuthor(author);
		libraryRepository.Save();

		var authorToReturn = mapper.Map<AuthorDTO>(author);

		return CreatedAtAction(nameof(GetById), new { authorId = authorToReturn.Id }, authorToReturn);
	}

	[HttpOptions]
	public IActionResult GetAuthorsOptions()
	{
		Response.Headers.Add("Allow", "GET,OPTIONS,POST");
		// Respose.Body describing available options
		return Ok();
	}

	[HttpDelete]
	[Route("{authorId}")]
	public IActionResult DeleteAuthor(Guid authorId)
	{
		var author = libraryRepository.GetAuthor(authorId);

		if (author == null)
			return NotFound(new { IsSuccess = false, Message = nameof(authorId) + " doesnt exist" });

		libraryRepository.DeleteAuthor(author);
		libraryRepository.Save();

		return NoContent();
	}

	private string CreateAuthorsResourceUri(AuthorsResourceParameters parameters, ResourceUriType type)
	{
		HttpContext.Request.RouteValues.TryGetValue("action", out var value);

		switch (type)
		{
			case ResourceUriType.PreviousPage:
				return Url.ActionLink(value?.ToString(),
					values: new
					{
						pageNumber = parameters.PageNumber - 1,
						pageSize = parameters.PageSize,
						mainCategory = parameters.MainCategory,
						searchQuery = parameters.SearchQuery,
						orderBy = parameters.OrderBy,
						fields = parameters.Fields
					})!;

			case ResourceUriType.NextPage:
				return Url.ActionLink(value?.ToString(),
					values: new
					{
						pageNumber = parameters.PageNumber + 1,
						pageSize = parameters.PageSize,
						mainCategory = parameters.MainCategory,
						searchQuery = parameters.SearchQuery,
						orderBy = parameters.OrderBy,
						fields = parameters.Fields
					})!;

			default:
				return Url.ActionLink(nameof(GetAuthors),
					values: new
					{
						pageNumber = parameters.PageNumber,
						pageSize = parameters.PageSize,
						mainCategory = parameters.MainCategory,
						searchQuery = parameters.SearchQuery,
						orderBy = parameters.OrderBy,
						fields = parameters.Fields
					})!;
		}
	}

	private IEnumerable<LinkDTO> CreateLinksForAuthor(Guid authorId, string fields)
	{
		var links = new List<LinkDTO>();

		if (string.IsNullOrWhiteSpace(fields))
		{
			links.Add(new LinkDTO(Url.ActionLink(nameof(GetById), values: new { authorId })!, "self", "GET"));
		}
		else
		{
			links.Add(new LinkDTO(Url.ActionLink(nameof(GetById), values: new { authorId, fields })!, "self", "GET"));
		}

		links.Add(new LinkDTO(Url.ActionLink(nameof(DeleteAuthor), values: new { authorId })!, "delete_author", "DELETE"));
		links.Add(new LinkDTO(Url.Link("CreateCourse", values: new { authorId })!, "create_course_for_author", "POST"));
		links.Add(new LinkDTO(Url.Link("GetCoursesForAuthor", values: new { authorId })!, "courses", "GET"));

		return links;
	}
}

public class CallCenterRowReportExcel
{
	public string Title { get; set; }
	public List<string> Values { get; set; }
}