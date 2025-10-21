using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSSafeAreaPage : ContentPage
	{
		public iOSSafeAreaPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			On<iOS>().SetUseSafeArea(false);
			((Button)sender).IsEnabled = false;
		}
	}
}
