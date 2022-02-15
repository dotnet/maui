namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Preferences.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Preferences']/Docs" />
	public class PreferencesImplementation : IPreferences
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
