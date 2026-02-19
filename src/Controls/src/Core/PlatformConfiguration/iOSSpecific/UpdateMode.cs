namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	/// <summary>Specifies when picker controls update their selected value during user interaction on iOS.</summary>
	public enum UpdateMode
	{
		/// <summary>Updates the value immediately as the user scrolls.</summary>
		Immediately,
		/// <summary>Updates the value only after the user finishes scrolling.</summary>
		WhenFinished
	}
}