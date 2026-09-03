using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class AndroidSoftInputModeAdjustPage : ContentPage
	{
		public AndroidSoftInputModeAdjustPage()
		{
			InitializeComponent();
		}

		void OnPanButtonClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Application.Current!.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Pan);
		}

		void OnResizeButtonClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Application.Current!.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
		}
	}
}
