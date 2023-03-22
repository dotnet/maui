#nullable enable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Optional setting to open the browser with.
	/// </summary>
	/// <remarks>Not all settings apply to all operating systems. Check documentation for more information.</remarks>
	public class BrowserLaunchOptions
	{
		/// <summary>
		/// Gets or sets the preferred color of the toolbar background of the in-app browser.
		/// </summary>
		/// <remarks>This setting only applies to iOS and Android.</remarks>
		public Color? PreferredToolbarColor { get; set; }

		/// <summary>
		/// Gets or sets the preferred color of the controls on the in-app browser.
		/// </summary>
		/// <remarks>This setting only applies to iOS.</remarks>
		public Color? PreferredControlColor { get; set; }

		/// <summary>
		/// Gets or sets how the browser should be launched.
		/// </summary>
		/// <remarks>The default value is <see cref="BrowserLaunchMode.SystemPreferred"/>.</remarks>
		public BrowserLaunchMode LaunchMode { get; set; } = BrowserLaunchMode.SystemPreferred;

		/// <summary>
		/// Gets or sets the preferred mode for the title display.
		/// </summary>
		/// <remarks>The default value is <see cref="BrowserTitleMode.Default"/>. This setting only applies to Android.</remarks>
		public BrowserTitleMode TitleMode { get; set; } = BrowserTitleMode.Default;

		/// <summary>
		/// Gets or sets additional launch flags that may or may not take effect based on the device and <see cref="LaunchMode"/>.
		/// </summary>
		/// <remarks>The default value is <see cref="BrowserLaunchFlags.None"/>. Not all flags work on all platforms, check the flag descriptions.</remarks>
		public BrowserLaunchFlags Flags { get; set; } = BrowserLaunchFlags.None;

		internal bool HasFlag(BrowserLaunchFlags flag) => Flags.HasFlag(flag);
	}

	/// <summary>
	/// Additional flags that can be set to control how the browser opens.
	/// </summary>
	[Flags]
	public enum BrowserLaunchFlags
	{
		/// <summary>No additional flags. This is the default.</summary>
		None = 0,

		/// <summary>Only applicable to Android: launches a new activity adjacent to the current activity if available.</summary>
		LaunchAdjacent = 1,

		/// <summary>Only applicable to iOS: launches the browser as a page sheet with the system preferred browser where supported.</summary>
		PresentAsPageSheet = 2,

		/// <summary>Only applicable to iOS: launches the browser as a form sheet with the system preferred browser where supported.</summary>
		PresentAsFormSheet = 4
	}
}
