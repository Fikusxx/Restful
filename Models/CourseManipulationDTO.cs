using Library.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Library.Models;

[TitleMustBeDifferentFromDescription(ErrorMessage = "Title must be different from the Description")]
public abstract class CourseManipulationDTO
{
	[Required]
	[MaxLength(100)]
	public string Title { get; set; }

	[MaxLength(1500)]
	public virtual string? Description { get; set; }
}
