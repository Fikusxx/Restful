
namespace Library.Helpers;

public static class DateTimeOffsetExtensions
{
	public static int GetCurrentAge(this DateTimeOffset date)
	{
		var currentDate = DateTime.UtcNow;
		int age = currentDate.Year - date.Year;

		if (currentDate < date.AddYears(age))
			age--;

		return age;
	}
}
