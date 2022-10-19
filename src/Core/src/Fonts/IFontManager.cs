namespace Microsoft.Maui
{
	/// <summary>
	/// The <see cref="FontManager"/> handles all fonts, font families and font sizes througout the application.
	/// </summary>
	public partial interface IFontManager
	{
		/// <summary>
		/// The default fontsize for the operating system.
		/// </summary>
		double DefaultFontSize { get; }
	}
}