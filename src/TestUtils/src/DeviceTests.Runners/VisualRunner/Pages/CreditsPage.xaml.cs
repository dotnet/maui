using Microsoft.Maui.Controls;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages
{
	partial class CreditsPage : ContentPage
	{
		public CreditsPage()
		{
			InitializeComponent();

			// Load about text
			var html = "<html><body><b>xUnit Device Runner</b><br>Copyright &copy; 2015<br>Outercurve Foundation<br>All rights reserved.<br><br>Author: Oren Novotny<hr /></body></html>";

			WebView.Source = new HtmlWebViewSource { Html = html };
		}
	}
}
