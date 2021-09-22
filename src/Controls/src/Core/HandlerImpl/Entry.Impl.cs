namespace Microsoft.Maui.Controls
{
	public partial class Entry : IEntry
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

		void IEntry.Completed()
		{
			(this as IEntryController).SendCompleted();
		}
	}
}