using Library.API.Entities;
using Library.Models;

namespace Library.Services;

public class PropertyMappingService : IPropertyMappingService
{
	// Can add lazy init to avoid initializing this on creating
	private Dictionary<string, PropertyMappingValue> authorProperties =
		new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase) // ignore lower / capital casing
	{
		{"Id", new PropertyMappingValue(new List<string>() {"Id"}) },
		{"MainCategory", new PropertyMappingValue(new List<string>() {"MainCategory"}) },
		{"Age", new PropertyMappingValue(new List<string>() {"DateOfBirth"}, true) },
		{"Name", new PropertyMappingValue(new List<string>() {"FirstName", "LastName"}) }
	};

	private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

	public PropertyMappingService()
	{
		propertyMappings.Add(new PropertyMapping<AuthorDTO, Author>(authorProperties));
	}

	public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
	{
		// if we dont need any sorting
		if (string.IsNullOrWhiteSpace(fields))
			return true;

		// if we do - split fields
		var propertyMapping = GetPropertyMapping<TSource, TDestination>();
		var fieldsAfterSplit = fields.Split(',');

		// check if propertyMappings contains such value as a key
		foreach (var field in fieldsAfterSplit)
		{
			var trimmedField = field.Trim();
			var indexOfFirstSpace = trimmedField.IndexOf(' ');
			var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

			if (propertyMapping.ContainsKey(propertyName) == false)
				return false;
		}

		return true;
	}

	public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
	{
		var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

		if (matchingMapping.Count() == 1)
			return matchingMapping.First().MappingDictionary;

		throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}>");
	}
}
