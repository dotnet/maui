using Microsoft.Maui.Storage;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

sealed class FakePreferences : IPreferences
{
	public Dictionary<string, object> Values { get; } = new();
	readonly Dictionary<string, Dictionary<string, object>> _stores = new();
	public string? LastSharedName { get; private set; }
	public int ClearCalls { get; private set; }
	public Exception? Error { get; set; }
	public int Calls { get; private set; }
	public string? FailingSetKey { get; set; }
	public bool WriteBeforeFailure { get; set; }
	internal Dictionary<string, object> Store(string? sharedName)
	{
		if (sharedName is null)
			return Values;
		if (!_stores.TryGetValue(sharedName, out var store))
			_stores[sharedName] = store = new();
		return store;
	}
	void Called(string? sharedName)
	{
		Calls++;
		LastSharedName = sharedName;
		if (Error is not null)
			throw Error;
	}
	public bool ContainsKey(string key, string? sharedName = null) { Called(sharedName); return Store(sharedName).ContainsKey(key); }
	public T Get<T>(string key, T defaultValue, string? sharedName = null) { Called(sharedName); return Store(sharedName).TryGetValue(key, out var value) ? (T)value : defaultValue; }
	public void Set<T>(string key, T value, string? sharedName = null)
	{
		Called(sharedName);
		if (key == FailingSetKey)
		{
			if (WriteBeforeFailure)
				Store(sharedName)[key] = value!;
			throw new IOException("Expected preference store failure.");
		}
		Store(sharedName)[key] = value!;
	}
	public void Remove(string key, string? sharedName = null) { Called(sharedName); Store(sharedName).Remove(key); }
	public void Clear(string? sharedName = null) { Called(sharedName); ClearCalls++; Store(sharedName).Clear(); }
}
