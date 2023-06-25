using StackExchange.Redis;
using System.Text.Json;

namespace Library.Caching;

public class CacheService : ICacheService
{
	private readonly IDatabase cacheDb;

    public CacheService(IConnectionMultiplexer muxer)
    {
		this.cacheDb = muxer.GetDatabase();
	}

	public T GetData<T>(string key)
	{
		var value = cacheDb.StringGet(key);

		if (string.IsNullOrEmpty(value))
			return default;

		try
		{
			return JsonSerializer.Deserialize<T>(value);
		}
		catch (Exception)
		{ }

		return default;
	}

	public bool RemoveData(string key)
	{
		var exist = cacheDb.KeyExists(key);

		if (exist == false)
			return false;

		return cacheDb.KeyDelete(key);
	}

	public bool SetData<T>(string key, T data, DateTimeOffset expirationTime)
	{
		var expiry = expirationTime.DateTime.Subtract(DateTime.UtcNow);
		var value = JsonSerializer.Serialize(data);
		
		return cacheDb.StringSet(key, value, expiry);
	}
}
