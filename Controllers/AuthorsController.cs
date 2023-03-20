using Library.API.Services;
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
        var authors = libraryRepository.GetAuthors();

        return Ok(authors);
    }

    [HttpGet]
    [Route("{authorId}")]
    public IActionResult GetById(Guid authorId)
    {
        var author = libraryRepository.GetAuthor(authorId);

        if (author == null) 
            return NotFound(new {IsSuccess = false, Message  = nameof(authorId) + " doesnt exist"});

        return Ok(author);
    }
}
