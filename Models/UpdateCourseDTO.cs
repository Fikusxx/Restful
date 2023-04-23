using System.ComponentModel.DataAnnotations;

namespace Library.Models;


public class UpdateCourseDTO : CourseManipulationDTO
{
	[Required]
	public override string Description { get => base.Description; set => base.Description = value; }
}
