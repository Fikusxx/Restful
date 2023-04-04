using Library.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Library.Models;

[TitleMustBeDifferentFromDescription(ErrorMessage = "Title must be different from the Description")]
public class CreateCourseDTO // : IValidatableObject
{
	[Required]
	[MaxLength(100)]
	public string Title { get; set; }

	[MaxLength(1500)]
	public string Description { get; set; }

	//public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	//{
	//	if (Title == Description)
	//	{
	//		yield return new ValidationResult("Title must be different from the Description", new[] { nameof(CreateCourseDTO) });
	//	}
	//}
}
