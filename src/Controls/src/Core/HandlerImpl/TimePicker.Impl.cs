namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize, enableScaling: FontScalingEnabled).WithAttributes(FontAttributes);
	}
}