namespace Microsoft.Maui.Controls
{
	public partial class DatePicker : IDatePicker
	{
		string _text;

		string IDatePicker.Text
		{
			get { return _text; }
			set
			{
				_text = value;
				Handler?.UpdateValue(nameof(DatePicker.TextTransform));
			}
		}

		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}