namespace Library.Models;

public class CourseDTO
{
	public Guid Id { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public Guid AuthorId { get; set; }
}
