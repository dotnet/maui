namespace Microsoft.Maui.Controls
{
	public partial class Picker : IPicker
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		int IPicker.GetCount() => Items?.Count ?? ItemsSource?.Count ?? 0;

		string IPicker.DisplayFor(int index)
		{
			if (index < 0)
				return "";
			if (Items?.Count < index)
				return Items[index];
			if (ItemsSource?.Count < index)
				GetDisplayMember(ItemsSource[index]);
			return "";
		}
	}
}