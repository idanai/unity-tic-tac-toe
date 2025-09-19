public interface ISaveSystem
{
	bool Exists(string key);
	void Delete(string key);
	void Save<T>(string key, in T value);
	bool TryLoad<T>(string key, out T value);
}