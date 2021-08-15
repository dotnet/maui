namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}