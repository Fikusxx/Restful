using AutoMapper;
using Library.API.Entities;
using Library.API.Services;
using Library.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

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

	[HttpGet(Name = "GetCoursesForAuthor")]
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

	[HttpPost(Name = "CreateCourse")]
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

	[HttpPut]
	[Route("{courseId}")]
	public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, UpdateCourseDTO updateCourseDTO)
	{
		if (libraryRepository.AuthorExists(authorId) == false)
			return NotFound("Author doesnt exist");

		var course = libraryRepository.GetCourse(authorId, courseId);

		if (course == null)
		{
			course = mapper.Map<Course>(updateCourseDTO);
			course.Id = courseId;
			libraryRepository.AddCourse(authorId, course);
			libraryRepository.Save();

			return CreatedAtAction(nameof(GetCourseForAuthor), new { authorId = authorId, courseId = course.Id }, course);
		}

		mapper.Map(updateCourseDTO, course);
		libraryRepository.UpdateCourse(course); // no implementation
		libraryRepository.Save();

		return NoContent();
	}

	[HttpPatch]
	[Route("{courseId}")]
	public IActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId, JsonPatchDocument<UpdateCourseDTO> patch)
	{
		if (libraryRepository.AuthorExists(authorId) == false)
			return NotFound("Author doesnt exist");

		var course = libraryRepository.GetCourse(authorId, courseId);

		if (course == null)
			return NotFound("Course doesnt exist");

		var courseToPatch = mapper.Map<UpdateCourseDTO>(course);
		patch.ApplyTo(courseToPatch, ModelState);

		if (TryValidateModel(courseToPatch) == false)
		{
			return ValidationProblem(ModelState);
		}

		mapper.Map(courseToPatch, course);
		libraryRepository.UpdateCourse(course); // no implementation
		libraryRepository.Save();

		return NoContent();
	}

	[HttpDelete]
	[Route("{courseId}")]
	public IActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
	{
		var course = libraryRepository.GetCourse(authorId, courseId);

		if (course == null)
			return NotFound("Course doesnt exist");

		libraryRepository.DeleteCourse(course);
		libraryRepository.Save();

		return NoContent();
	}

	public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
	{
		var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;

		return (ActionResult)options.InvalidModelStateResponseFactory(ControllerContext);
	}
}
