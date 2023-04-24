using System.Reflection;

namespace Library.Services;

public class PropertyCheckerService : IPropertyCheckerService
{
	public bool TypeHasProperties<T>(string fields)
	{
		if (string.IsNullOrWhiteSpace(fields))
			return true;

		var fieldsAfterSplit = fields.Split(',');

		// check if propertyMappings contains such value as a key
		foreach (var field in fieldsAfterSplit)
		{
			var propertyName = field.Trim();

			var propertyInfo = typeof(T).GetProperty(propertyName,
				BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

			if (propertyInfo == null)
				return false;
		}

		return true;
	}
}
