namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to know if need to adapt the content on an area of the screen.
	/// </summary>
	public interface ISafeAreaView
	{
		/// <summary>
		/// Ensure that content is positioned on an area of the screen that is safe for all devices.
		/// </summary>
		bool IgnoreSafeArea { get; }
	}
}
