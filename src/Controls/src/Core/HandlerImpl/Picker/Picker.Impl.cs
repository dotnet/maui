#nullable disable
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class Picker : IPicker
	{
		Font ITextStyle.Font => this.ToFont();

		IList<string> IPicker.Items => Items;

		int IItemDelegate<string>.GetCount() => Items?.Count ?? ItemsSource?.Count ?? 0;

		string IItemDelegate<string>.GetItem(int index)
		{
			if (index < 0)
				return string.Empty;
			if (index < Items?.Count)
				return GetItem(index);
			if (index < ItemsSource?.Count)
				return GetDisplayMember(ItemsSource[index]);
			return string.Empty;
		}

		string GetItem(int index)
		{
			if (index < Items?.Count)
			{
				var item = Items[index];
				return item ?? string.Empty;
			}

			return string.Empty;
		}
	}
}