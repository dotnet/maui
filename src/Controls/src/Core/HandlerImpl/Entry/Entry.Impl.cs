namespace Microsoft.Maui.Controls
{
	public partial class Entry : IEntry
	{
		Font ITextStyle.Font => this.ToFont();

		void IEntry.Completed()
		{
			(this as IEntryController).SendCompleted();
		}
	}
}