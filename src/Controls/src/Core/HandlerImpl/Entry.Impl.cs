namespace Microsoft.Maui.Controls
{
	public partial class Entry : IEntry
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		void IEntry.Completed()
		{
			(this as IEntryController).SendCompleted();
		}
	}
}