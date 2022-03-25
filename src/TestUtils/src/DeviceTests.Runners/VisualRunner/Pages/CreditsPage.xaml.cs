#nullable enable
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

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