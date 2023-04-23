using Library.API.DbContexts;
using Library.API.Entities;
using Library.Helpers;
using Library.Models;
using Library.Resources;
using Library.Services;

namespace Library.API.Services;

public class LibraryRepository : ILibraryRepository, IDisposable
{
	private readonly LibraryContext _context;
	private readonly IPropertyMappingService propertyMappingService;

	public LibraryRepository(LibraryContext context, IPropertyMappingService propertyMappingService)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		this.propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
	}

	public void AddCourse(Guid authorId, Course course)
	{
		if (authorId == Guid.Empty)
		{
			throw new ArgumentNullException(nameof(authorId));
		}

		if (course == null)
		{
			throw new ArgumentNullException(nameof(course));
		}

		// always set the AuthorId to the passed-in authorId
		course.AuthorId = authorId;
		_context.Courses.Add(course);
	}

	public void DeleteCourse(Course course)
	{
		_context.Courses.Remove(course);
	}

	public Course GetCourse(Guid authorId, Guid courseId)
	{
		if (authorId == Guid.Empty)
		{
			throw new ArgumentNullException(nameof(authorId));
		}

		if (courseId == Guid.Empty)
		{
			throw new ArgumentNullException(nameof(courseId));
		}

		return _context.Courses
		  .FirstOrDefault(c => c.AuthorId == authorId && c.Id == courseId);
	}

	public IEnumerable<Course> GetCourses(Guid authorId)
	{
		if (authorId == Guid.Empty)
		{
			throw new ArgumentNullException(nameof(authorId));
		}

		return _context.Courses
					.Where(c => c.AuthorId == authorId)
					.OrderBy(c => c.Title).ToList();
	}

	public void UpdateCourse(Course course)
	{
		// no code in this implementation
	}

	public void AddAuthor(Author author)
	{
		if (author == null)
		{
			throw new ArgumentNullException(nameof(author));
		}

		author.Id = Guid.NewGuid();

		foreach (var course in author.Courses)
		{
			course.Id = Guid.NewGuid();
		}

		_context.Authors.Add(author);
	}

	public bool AuthorExists(Guid authorId)
	{
		if (authorId == Guid.Empty)
		{
			throw new ArgumentNullException(nameof(authorId));
		}

		return _context.Authors.Any(a => a.Id == authorId);
	}

	public void DeleteAuthor(Author author)
	{
		if (author == null)
		{
			throw new ArgumentNullException(nameof(author));
		}

		_context.Authors.Remove(author);
	}

	public Author GetAuthor(Guid authorId)
	{
		if (authorId == Guid.Empty)
		{
			throw new ArgumentNullException(nameof(authorId));
		}

		return _context.Authors.FirstOrDefault(a => a.Id == authorId);
	}

	public IEnumerable<Author> GetAuthors()
	{
		return _context.Authors.ToList<Author>();
	}

	public PagedList<Author> GetAuthors(AuthorsResourceParameters parameters)
	{
		if (parameters == null)
			throw new ArgumentNullException(nameof(parameters));

		var collection = _context.Authors.AsQueryable();

		if (string.IsNullOrWhiteSpace(parameters.MainCategory) == false)
		{
			var mainCategory = parameters.MainCategory.Trim();
			collection = collection.Where(x => x.MainCategory == mainCategory);
		}

		if (string.IsNullOrWhiteSpace(parameters.SearchQuery) == false)
		{
			var searchQuery = parameters.SearchQuery.Trim();
			collection = collection.Where(x => x.MainCategory.Contains(searchQuery)
			|| x.FirstName.Contains(searchQuery)
			|| x.LastName.Contains(searchQuery));
		}

		if(string.IsNullOrWhiteSpace(parameters.OrderBy) == false)
		{
			var authorPropertyMappingDictionary = propertyMappingService.GetPropertyMapping<AuthorDTO, Author>();
			collection = collection.ApplySort(parameters.OrderBy, authorPropertyMappingDictionary);
		}

		return PagedList<Author>.ToPagedList(collection, parameters.PageNumber, parameters.PageSize);
	}

	public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
	{
		if (authorIds == null)
		{
			throw new ArgumentNullException(nameof(authorIds));
		}

		return _context.Authors.Where(a => authorIds.Contains(a.Id))
			.OrderBy(a => a.FirstName)
			.OrderBy(a => a.LastName)
			.ToList();
	}

	public void UpdateAuthor(Author author)
	{
		// no code in this implementation
	}

	public bool Save()
	{
		return (_context.SaveChanges() >= 0);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			// dispose resources when needed
		}
	}
}
