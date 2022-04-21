using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs" />
	public partial class Label : ILabel
	{
		Color ITextStyle.TextColor
		{
			get => TextColor ??
				DefaultStyles.GetTextColor(this)?.Value as Color;
		}
		
		Font ITextStyle.Font => this.ToFont();
	}
}
