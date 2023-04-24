using System.Dynamic;
using System.Reflection;

namespace Library.Helpers;

public static class ObjectExtensions
{
	public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
	{
		if (source == null)
			throw new ArgumentNullException(nameof(source));

		var dataShapedObject = new ExpandoObject();

		var propertyInfoList = new List<PropertyInfo>();

		if (string.IsNullOrWhiteSpace(fields)) // if we didnt get any fields passed
		{
			var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
			propertyInfoList.AddRange(propertyInfos);
		}
		else // if we got fields separated by ','
		{
			var fieldsAfterSplit = fields.Split(',');

			foreach (var field in fieldsAfterSplit)
			{
				var propertyName = field.Trim();

				var propertyInfo = typeof(TSource).GetProperty(propertyName,
					BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

				if (propertyInfo == null)
					throw new Exception($"Property {propertyName} was not found on {typeof(TSource)}");

				propertyInfoList.Add(propertyInfo);
			}
		}

		foreach (var propertyInfo in propertyInfoList)
		{
			var propertyValue = propertyInfo.GetValue(source);
			dataShapedObject.TryAdd(propertyInfo.Name, propertyValue);
		}

		return dataShapedObject;
	}
}