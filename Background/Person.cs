using System.ComponentModel.DataAnnotations;

namespace Library.Background;


public interface IBaseEntity
{
	public int Id { get; set; }

	public DateTime LastModified { get; set; }
}
public class BaseEntity
{
	public int Id { get; set; }
	
	public DateTime LastModified { get; set; }
}

public class Person : IBaseEntity
{
	public int Id { get; set; }

	public DateTime LastModified { get; set; }

	[Required]
	public string Name { get; set; } = "";

	[Range(1, 100)]
	public int Age { get; set; }
}