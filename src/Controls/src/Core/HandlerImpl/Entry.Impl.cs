namespace Microsoft.Maui.Controls
{
	public partial class Entry : IEntry
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();

		void IEntry.Completed()
		{
			(this as IEntryController).SendCompleted();
		}
	}
}