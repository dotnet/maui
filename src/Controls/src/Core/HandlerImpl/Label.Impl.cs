namespace Microsoft.Maui.Controls
{
	public partial class Label : ILabel
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize, enableScaling: FontScalingEnabled).WithAttributes(FontAttributes);
	}
}