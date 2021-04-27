#nullable enable
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiComboBox : ComboBox
	{
		public MauiComboBox()
		{
			DefaultStyleKey = typeof(MauiComboBox);

			DropDownOpened += OnMauiComboBoxDropDownOpened;
			SelectionChanged += OnMauiComboBoxSelectionChanged;
		}

		void OnMauiComboBoxDropDownOpened(object? sender, object e)
		{
			MinWidth = ActualWidth;
		}

		void OnMauiComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			MinWidth = 0;
		}
	}
}