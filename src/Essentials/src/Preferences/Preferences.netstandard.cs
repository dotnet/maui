using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{
	class PreferencesImplementation : IPreferences
	{
		public bool ContainsKey(string key, string sharedName) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Remove(string key, string sharedName) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Clear(string sharedName) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Set<T>(string key, T value, string sharedName) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public T Get<T>(string key, T defaultValue, string sharedName) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
