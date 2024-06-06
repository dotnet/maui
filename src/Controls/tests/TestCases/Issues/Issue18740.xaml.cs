using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18740, "Virtual keyboard appears with focus on Entry", PlatformAffected.Android)]
	public class Issue18740NavPage : NavigationPage
	{
		public Issue18740NavPage() : base(new Issue18740())
		{

		}
	}

	public partial class Issue18740 : ContentPage
	{
		public Issue18740() : base()
		{
			InitializeComponent();
		}

		async void OnEntryButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Issue18740Entry());
		}

		async void OnEditorButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Issue18740Editor());
		}

		async void OnSearchBarButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new Issue18740SearchBar());
		}
	}
}