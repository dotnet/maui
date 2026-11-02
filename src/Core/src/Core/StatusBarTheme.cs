namespace Microsoft.Maui
{
	/// <summary>
	/// Controls the theme of OS-drawn status bar content (icons, text) on mobile platforms (Android, iOS).
	/// Analogous to <see cref="ApplicationModel.AppTheme"/> — <see cref="Light"/> means light background
	/// with dark icons, <see cref="Dark"/> means dark background with light icons.
	/// On desktop platforms (Windows, macCatalyst) this is a no-op since there are no OS-drawn status bar icons.
	/// </summary>
	public enum StatusBarTheme
	{
		/// <summary>
		/// Automatically follow the current app theme. This is the default
		/// and preserves existing behavior.
		/// </summary>
		Default,

		/// <summary>
		/// The status bar area has a light background — use dark icons/text for contrast.
		/// Maps to: Android <c>AppearanceLightStatusBars=true</c>, iOS <c>UIStatusBarStyle.DarkContent</c>.
		/// </summary>
		Light,

		/// <summary>
		/// The status bar area has a dark background — use light/white icons/text for contrast.
		/// Maps to: Android <c>AppearanceLightStatusBars=false</c>, iOS <c>UIStatusBarStyle.LightContent</c>.
		/// </summary>
		Dark
	}
}
