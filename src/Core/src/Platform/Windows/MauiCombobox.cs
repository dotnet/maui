#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiComboBox : ComboBox
	{
		public MauiComboBox()
		{
			DefaultStyleKey = typeof(MauiComboBox);

			DropDownOpened += FormsComboBoxDropDownOpened;
			SelectionChanged += FormsComboBoxSelectionChanged;
		}

		void FormsComboBoxDropDownOpened(object? sender, object e)
		{
			MinWidth = ActualWidth;
		}

		void FormsComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			MinWidth = 0;
		}
	}
}