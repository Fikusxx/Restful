namespace Library.Caching;

public interface ICacheService
{
	T GetData<T>(string key);	
	bool SetData<T>(string key, T data, DateTimeOffset expirationTime);
	bool RemoveData(string key);
}
