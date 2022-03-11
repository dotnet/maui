namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/TimePicker.xml" path="Type[@FullName='Microsoft.Maui.Controls.TimePicker']/Docs" />
	public partial class TimePicker : ITimePicker
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}