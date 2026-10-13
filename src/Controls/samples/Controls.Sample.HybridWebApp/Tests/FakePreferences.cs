using Microsoft.Maui.Storage;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

sealed class FakePreferences : IPreferences
{
	public Dictionary<string, object> Values { get; } = new();
	public int Calls { get; private set; }
	public bool ContainsKey(string key, string? sharedName = null) { Calls++; return Values.ContainsKey(key); }
	public T Get<T>(string key, T defaultValue, string? sharedName = null) { Calls++; return Values.TryGetValue(key, out var value) ? (T)value : defaultValue; }
	public void Set<T>(string key, T value, string? sharedName = null) { Calls++; Values[key] = value!; }
	public void Remove(string key, string? sharedName = null) { Calls++; Values.Remove(key); }
	public void Clear(string? sharedName = null) => throw new InvalidOperationException("The provider must not expose Clear.");
}
