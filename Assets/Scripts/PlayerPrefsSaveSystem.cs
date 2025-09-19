using UnityEngine;

public class PlayerPrefsSaveSystem : ISaveSystem
{
	public bool Exists(string key) => PlayerPrefs.HasKey(key);

	public void Delete(string key) => PlayerPrefs.DeleteKey(key);

	// Note: if value is serialized as an empty string, TryLoad will return false
	public void Save<T>(string key, in T value)
	{
		var s = JsonUtility.ToJson(value); // potential boxing here
		PlayerPrefs.SetString(key, s);
	}

	public bool TryLoad<T>(string key, out T value)
	{
		var s = PlayerPrefs.GetString(key);
		if (s == string.Empty)
		{
			value = default;
			return false;
		}
		value = JsonUtility.FromJson<T>(s); // potential un-boxing here
		return true;
	}
}