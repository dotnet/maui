namespace Microsoft.Maui.Controls
{
	public partial class Picker : IPicker
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		int IITemDelegate<string>.GetCount() => Items?.Count ?? ItemsSource?.Count ?? 0;

		string IITemDelegate<string>.GetItem(int index)
		{
			if (index < 0)
				return "";
			if (index < Items?.Count)
				return Items[index];
			if (index < ItemsSource?.Count)
				return GetDisplayMember(ItemsSource[index]);
			return "";
		}

	}
}