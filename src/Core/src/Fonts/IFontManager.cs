namespace Microsoft.Maui
{
	/// <summary>
	/// The <see cref="FontManager"/> handles all fonts, font families and font sizes throughout the application.
	/// </summary>
	public partial interface IFontManager
	{
		/// <summary>
		/// The default font size for the operating system.
		/// </summary>
		double DefaultFontSize { get; }
	}
}