namespace Microsoft.Maui.Controls
{
	public partial class TimePicker : ITimePicker
	{
		string _text;

		string ITimePicker.Text
		{
			get { return _text; }
			set
			{
				_text = value;
				Handler?.UpdateValue(nameof(TimePicker.TextTransform));
			}
		}

		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}