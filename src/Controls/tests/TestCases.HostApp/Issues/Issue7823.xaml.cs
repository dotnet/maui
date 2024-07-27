using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 7823, "In a ToolbarItems, if an item has no icon but just text, MAUI uses the icon from the previous page in the Navigation", PlatformAffected.Android)]
	public class Issue7823NavigationPage : NavigationPage
	{
		public Issue7823NavigationPage() : base(new Issue7823())
		{

		}
	}

	public partial class Issue7823 : ContentPage
	{
		public Issue7823()
		{
			InitializeComponent();
		}

		async void OnToolbarItemClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Issue7823Page2());
		}

		async void OnButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Issue7823Page2());
		}
	}
}