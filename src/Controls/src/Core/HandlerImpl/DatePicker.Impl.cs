using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DatePicker.xml" path="Type[@FullName='Microsoft.Maui.Controls.DatePicker']/Docs" />
	public partial class DatePicker : IDatePicker
	{
		Font ITextStyle.Font => this.ToFont();
		Color ITextStyle.TextColor
		{
			get => TextColor ??
				DefaultStyles.GetColor(this, TextColorProperty)?.Value as Color;
		}
	}
}