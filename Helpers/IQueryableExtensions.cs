using Library.Services;
using System.Linq.Dynamic.Core;

namespace Library.Helpers;

public static class IQueryableExtensions
{
	public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
		Dictionary<string, PropertyMappingValue> mappingDictionary)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		if (mappingDictionary == null)
			throw new ArgumentNullException(nameof(mappingDictionary));

		if (string.IsNullOrWhiteSpace(orderBy))
			return source;

		// orderBy=name desc, age asc...
		var orderByAfterSplit = orderBy.Split(',');

		// Whole reverse thing is because theres no ThenBy() method
		// Thus each OrderBy kinda overwrites previous OrderBy
		// And since first orderByClause should be primary sorting, we make it to be the last one to be executed
		foreach (var orderByClause in orderByAfterSplit.Reverse())
		{
			// remove white spaces
			var trimmedOrderByClause = orderByClause.Trim();

			// if order should be descending
			var orderDesc = trimmedOrderByClause.EndsWith(" desc");

			// get propertyName = "name desc", returns "name"
			var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
			var propertyName = indexOfFirstSpace == -1 ? trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

			// if there's not any matching property in the provided dict
			//if (mappingDictionary.ContainsKey(propertyName) == false)
			//	throw new ArgumentException($"Key mapping for {propertyName} is missing");

			var propertyMappingValue = mappingDictionary[propertyName];

			if (propertyMappingValue == null)
				throw new ArgumentNullException(nameof(propertyMappingValue));

			foreach (var destProperty in propertyMappingValue.DestinationProperties.Reverse())
			{
				if (propertyMappingValue.Revert)
					orderDesc = !orderDesc;

				source = source.OrderBy(destProperty + (orderDesc ? " descending" : " ascending"));
			}
		}

		return source;
	}
}
