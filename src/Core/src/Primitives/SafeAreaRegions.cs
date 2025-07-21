using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Specifies which platform safe area edges to obey for a layout or visual element.
	/// </summary>
	[Flags]
	public enum SafeAreaRegions
	{
		/// <summary>
		/// Content goes edge to edge with no safe area padding.
		/// </summary>
		None = 0,

		/// <summary>
		/// Always pad so content doesn't go under the soft input/keyboard.
		/// </summary>
		SoftInput = 1,

		/// <summary>
		/// Content flows under keyboard but stays out of top and bottom bars and notch.
		/// </summary>
		Container = 2,

		/// <summary>
		/// Default behavior - apply platform safe area insets.
		/// </summary>
		Default = 4,

		/// <summary>
		/// Obey all safe area insets - content will be positioned only in the safe area.
		/// This includes top and bottom bars, notch, and keyboard.
		/// </summary>
		All = int.MaxValue
	}
}