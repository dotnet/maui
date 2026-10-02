using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class AndroidListViewFastScrollPage : ContentPage
	{
		public AndroidListViewFastScrollPage()
		{
			InitializeComponent();
			BindingContext = new ListViewViewModel();
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			listView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetIsFastScrollEnabled(!listView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().IsFastScrollEnabled());
		}
	}
}
