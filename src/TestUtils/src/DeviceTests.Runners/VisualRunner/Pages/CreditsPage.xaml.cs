#nullable enable
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages
{
	partial class CreditsPage : ContentPage
	{
		public CreditsPage()
		{
			InitializeComponent();
		}

		void OnNavigating(object? sender, WebNavigatingEventArgs e)
		{
			Browser.OpenAsync(e.Url);

			e.Cancel = true;
		}
	}
}