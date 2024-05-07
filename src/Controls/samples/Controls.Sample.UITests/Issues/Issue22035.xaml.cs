using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22035, "[Android] CarouselView: VirtualView cannot be null here, when clearing and adding items on second navigation", PlatformAffected.Android)]
	public class Issue22035 : NavigationPage
	{
		public Issue22035() : base(new Issue22035Page1())
		{

		}
	}

	public partial class Issue22035Page1 : ContentPage
	{
		public Issue22035Page1()
		{
			InitializeComponent();
		}

		void OnNavigateClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Issue22035Page2());
		}
	}
}