using Library.Models;
using System.ComponentModel.DataAnnotations;

namespace Library.ValidationAttributes;

public class TitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
{
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		var course = (CourseManipulationDTO)validationContext.ObjectInstance;

		if (course.Title == course.Description)
		{
			return new ValidationResult(ErrorMessage, new[] { nameof(CourseManipulationDTO) });
		}

		return ValidationResult.Success;
	}
}
