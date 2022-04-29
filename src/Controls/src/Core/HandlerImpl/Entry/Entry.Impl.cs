using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Entry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Entry']/Docs" />
	public partial class Entry : IEntry
	{
		Font ITextStyle.Font => this.ToFont();

		Color ITextStyle.TextColor
		{
			get => TextColor ??
				DefaultStyles.GetColor(this, TextColorProperty)?.Value as Color;
		}

		Color IPlaceholder.PlaceholderColor
		{
			get => TextColor ??
				DefaultStyles.GetColor(this, PlaceholderColorProperty)?.Value as Color;
		}

		void IEntry.Completed()
		{
			(this as IEntryController).SendCompleted();
		}
	}
}