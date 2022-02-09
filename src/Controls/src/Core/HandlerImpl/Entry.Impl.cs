namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Entry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Entry']/Docs" />
	public partial class Entry : IEntry
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

		void IEntry.Completed()
		{
			(this as IEntryController).SendCompleted();
		}
	}
}