using UnityEngine;

public class TextSaveSystem : ISaveSystem
{
	private static string GetPath(string key) => key;//"Assets/" + key;

	public bool Exists(string key) => System.IO.File.Exists(GetPath(key));

	public void Delete(string key) => System.IO.File.Delete(GetPath(key));

	public void Save<T>(string key, in T value)
		=> System.IO.File.WriteAllText(GetPath(key), JsonUtility.ToJson(value));

	public bool TryLoad<T>(string key, out T value)
	{
		try
		{
			var s = System.IO.File.ReadAllText(GetPath(key));
			value = JsonUtility.FromJson<T>(s);
			return true;
		}
		catch (System.IO.FileNotFoundException)
		{
			Debug.Log("File not found: " + GetPath(key));
			value = default;
			return false;
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
			value = default;
			return false;
		}
	}
}