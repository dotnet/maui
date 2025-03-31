using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsListViewPage : ContentPage
	{
		public WindowsListViewPage()
		{
			InitializeComponent();
			BindingContext = new ListViewViewModel();
			UpdateLabel();
		}

		async void OnListViewItemTapped(object sender, ItemTappedEventArgs e)
		{
			await DisplayAlertAsync("Item Tapped", "ItemTapped event fired.", "OK");
		}

		async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{
			await DisplayAlertAsync("Tap Gesture Recognizer", "Tapped event fired.", "OK");
		}

		void OnToggleButtonClicked(object sender, EventArgs e)
		{
			switch (_listView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetSelectionMode())
			{
				case Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Accessible:
					_listView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetSelectionMode(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Inaccessible);
					break;
				case Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Inaccessible:
					_listView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetSelectionMode(Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.ListViewSelectionMode.Accessible);
					break;
			}
			UpdateLabel();
		}

		void UpdateLabel()
		{
			_label.Text = $"ListView SelectionMode: {_listView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetSelectionMode()}";
		}
	}
}
