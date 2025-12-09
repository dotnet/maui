using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18961, "Modal Page margin correct after Keyboard opens", PlatformAffected.Android)]
	public partial class Issue18961 : ContentPage
	{
		public Issue18961()
		{
			InitializeComponent();
		}

		async void OnButtonClicked(object sender, EventArgs args)
		{
			var scrollY = TestScrollView.Height;
			await TestScrollView.ScrollToAsync(0, scrollY, false);
		}
	}
}