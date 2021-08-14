using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class Entry : IEntry, INotifyFontChanging
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();

		void IEntry.Completed()
		{
			(this as IEntryController).SendCompleted();
		}

		void INotifyFontChanging.FontChanging()
		{
			// Null out the Maui font value so it will be recreated next time it's accessed
			_font = null;
		}
	}
}