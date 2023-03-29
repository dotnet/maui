#nullable enable
namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Launch type of the browser.
	/// </summary>
	/// <remarks>It's recommended to use the <see cref="BrowserLaunchMode.SystemPreferred"/> as it is the default and gracefully falls back if needed.</remarks>
	public enum BrowserLaunchMode
	{
		/// <summary>Launch the optimized system browser and stay inside of your application. Chrome Custom Tabs on Android and SFSafariViewController on iOS.</summary>
		SystemPreferred = 0,

		/// <summary>Use the default external launcher to open the browser outside of the app.</summary>
		External = 1
	}
}
