namespace Microsoft.Maui.Controls;

interface IClippedToBoundsElement
{
	/// <summary>
	/// Gets or sets a value which determines if the layout should clip its children to its bounds.
	/// The default value is <see langword="false"/>.
	/// </summary>
	bool IsClippedToBounds { get; set; }
}
