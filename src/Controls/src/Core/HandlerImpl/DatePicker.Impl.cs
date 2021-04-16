using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class DatePicker : IDatePicker
	{
		Font? _font;

		public IBrush Foreground { get; set; }

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
	}
}