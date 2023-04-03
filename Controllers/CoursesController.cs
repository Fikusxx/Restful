using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Library.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers;

[ApiController]
[Route("api/authors/{authorId}/courses")]
public class CoursesController : ControllerBase
{
	private readonly ILibraryRepository libraryRepository;
	private readonly IMapper mapper;

	public CoursesController(ILibraryRepository libraryRepository, IMapper mapper)
	{
		this.libraryRepository = libraryRepository;
		this.mapper = mapper;
	}

	[HttpGet]
	public ActionResult<IEnumerable<CourseDTO>> GetCoursesForAuthor(Guid authorId)
	{
		if (libraryRepository.AuthorExists(authorId) == false)
			return NotFound("Author doesnt exist");

		var courses = libraryRepository.GetCourses(authorId);
		var coursesDtos = mapper.Map<List<CourseDTO>>(courses);

		return Ok(coursesDtos);
	}

	[HttpGet]
	[Route("{courseId}")]
	public ActionResult<CourseDTO> GetCourseForAuthor(Guid authorId, Guid courseId)
	{
		if (libraryRepository.AuthorExists(authorId) == false)
			return NotFound("Author doesnt exist");

		var course = libraryRepository.GetCourse(authorId, courseId);

		if (course == null)
			return NotFound("Course doesnt exist");

		var courseDto = mapper.Map<CourseDTO>(course);

		return Ok(courseDto);
	}

	[HttpPost]
	public ActionResult<CourseDTO> CreateCourse(Guid authorId, CreateCourseDTO course)
	{
		if (libraryRepository.AuthorExists(authorId) == false)
			return NotFound("Author doesnt exist");

		var courseToAdd = mapper.Map<Course>(course);
		libraryRepository.AddCourse(authorId, courseToAdd);
		libraryRepository.Save();

		var courseToReturn = mapper.Map<CourseDTO>(courseToAdd);

		return CreatedAtAction(nameof(GetCourseForAuthor), new { authorId = authorId, courseId = courseToAdd.Id }, courseToReturn);
	}
}
