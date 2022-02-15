namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Picker.xml" path="Type[@FullName='Microsoft.Maui.Controls.Picker']/Docs" />
	public partial class Picker : IPicker
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

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
	}
}