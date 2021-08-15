namespace Microsoft.Maui.Controls
{
	public partial class DatePicker : IDatePicker
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}