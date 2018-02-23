namespace Microsoft.Caboodle
{
	/// <summary>
	/// Shared code between preferences
	/// Contains static methods and shared members
	/// </summary>
	public sealed partial class Preferences
	{
		public Preferences()
		{
		}

		public Preferences(string sharedName)
		{
			SharedName = sharedName;
		}

		public string SharedName { get; private set; }

		public string Get(string key, string defaultValue) =>
			Get<string>(key, defaultValue);
		public bool Get(string key, bool defaultValue) =>
			Get<bool>(key, defaultValue);
		public int Get(string key, int defaultValue) =>
			Get<int>(key, defaultValue);
		public double Get(string key, double defaultValue) =>
			Get<double>(key, defaultValue);
		public float Get(string key, float defaultValue) =>
			Get<float>(key, defaultValue);
		public long Get(string key, long defaultValue) =>
			Get<long>(key, defaultValue);
	}
}
