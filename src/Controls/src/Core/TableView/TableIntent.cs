namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies the visual intent of a <see cref="TableView"/>, which determines how it is rendered on each platform.
	/// </summary>
	public enum TableIntent
	{
		/// <summary>Indicates the <see cref="TableView"/> is for displaying a menu.</summary>
		Menu,
		/// <summary>Indicates the <see cref="TableView"/> is for displaying application settings.</summary>
		Settings,
		/// <summary>Indicates the <see cref="TableView"/> is for displaying a data entry form.</summary>
		Form,
		/// <summary>Indicates the <see cref="TableView"/> is for displaying tabular data.</summary>
		Data
	}
}