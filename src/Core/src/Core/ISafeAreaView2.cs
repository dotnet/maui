namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality for SafeAreaInsets that may be changed in the future.
	/// </summary>
	/// <remarks>
	/// This interface is only recognized on the iOS/Mac Catalyst platforms; other platforms will ignore it.
	/// </remarks>
	internal interface ISafeAreaView2
	{
		/// <summary>
		/// Internal property for the SafeAreaInsets Thickness that may be changed in the future.
		/// </summary>
		internal Thickness SafeAreaInsets { get; }
	}
}
