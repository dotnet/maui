using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "D11", "Editor font properties", PlatformAffected.All)]
	public partial class Issue18673 : ContentPage
	{
		public Issue18673()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			TestSlider.Value = 24;
		}
	}
}