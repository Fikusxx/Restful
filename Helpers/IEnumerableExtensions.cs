using System.Dynamic;
using System.Reflection;

namespace Library.Helpers;

public static class IEnumerableExtensions
{
	public static IEnumerable<ExpandoObject> ShapeData<T>(this IEnumerable<T> source, string fields)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		// holds expando object projection to <T> type
		var expandoList = new List<ExpandoObject>();

		var propertyInfoList = new List<PropertyInfo>();

		if (string.IsNullOrWhiteSpace(fields))
		{
			var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			propertyInfoList.AddRange(props);
		}
		else
		{
			var fieldsAfterSplit = fields.Split(',');

			foreach (var field in fieldsAfterSplit)
			{
				var propertyName = field.Trim();
				var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

				if (propertyInfo == null)
					throw new Exception($"Property {propertyName} was not found on {typeof(T)}");

				propertyInfoList.Add(propertyInfo);
			}
		}

		foreach (var item in source)
		{
			var expando = new ExpandoObject();

			foreach (var property in propertyInfoList)
			{
				var propertyValue = property.GetValue(item);
				expando.TryAdd(property.Name, propertyValue);
			}

			expandoList.Add(expando);
		}

		return expandoList;
	}
}
