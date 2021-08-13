namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();
	}
}