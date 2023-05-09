using System.ComponentModel.DataAnnotations;

namespace Library.Background;

public class Person
{
	[Required]
	public string Name { get; set; } = "";

	[Range(1, 100)]
	public int Age { get; set; }
}