using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSScrollViewPage : Microsoft.Maui.Controls.FlyoutPage
	{
		public iOSScrollViewPage()
		{
			InitializeComponent();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			scrollView.On<iOS>().SetShouldDelayContentTouches(!scrollView.On<iOS>().ShouldDelayContentTouches());
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}
