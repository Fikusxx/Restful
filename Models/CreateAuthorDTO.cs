namespace Library.Models;

public class CreateAuthorDTO
{
	public string FirstName { get; set; }	
	public string LastName { get; set; }
	public DateTimeOffset DateOfBirth { get; set; }
	public string MainCategory { get; set; }
	public ICollection<CreateCourseDTO> Courses { get; set; } = new List<CreateCourseDTO>();
}
