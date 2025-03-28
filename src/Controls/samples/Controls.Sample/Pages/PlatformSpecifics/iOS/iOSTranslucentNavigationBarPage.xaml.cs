using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSTranslucentNavigationBarPage : ContentPage
	{
		public iOSTranslucentNavigationBarPage()
		{
			InitializeComponent();
		}

		void OnTranslucentNavigationBarButtonClicked(object sender, EventArgs e)
		{
			(this.Window!.Page as Microsoft.Maui.Controls.NavigationPage)!.On<iOS>().SetIsNavigationBarTranslucent(!(this.Window!.Page as Microsoft.Maui.Controls.NavigationPage)!.On<iOS>().IsNavigationBarTranslucent());
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}
