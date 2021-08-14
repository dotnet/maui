using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class Picker : IPicker, INotifyFontChanging
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();

		int IItemDelegate<string>.GetCount() => Items?.Count ?? ItemsSource?.Count ?? 0;

		string IItemDelegate<string>.GetItem(int index)
		{
			if (index < 0)
				return "";
			if (index < Items?.Count)
				return Items[index];
			if (index < ItemsSource?.Count)
				return GetDisplayMember(ItemsSource[index]);
			return "";
		}

		void INotifyFontChanging.FontChanging()
		{
			// Null out the Maui font value so it will be recreated next time it's accessed
			_font = null;
		}
	}
}