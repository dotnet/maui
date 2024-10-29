using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25371, "OnNavigatedTo not called when navigating back to a specific page", PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue25371 : NavigationPage
	{
		public Issue25371() : base(new MainPage())
		{
		}
	}
	public class MainPage : ContentPage
	{
		Label label;
		public MainPage()
		{
			var stackLayout = new StackLayout();
		    label = new Label()
			{
				Text = "Welcome to Main page",
				AutomationId ="MainPageLabel"
			};

			var button = new Button() { Text = "MoveToNextPage", AutomationId="MoveToNextPage" };
			button.Clicked += Button_Clicked;
			stackLayout.Children.Add(label);
			stackLayout.Children.Add(button);
			Content = stackLayout;
			
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			label.Text = "OnNavigationTo method is called";
			base.OnNavigatedTo(args);
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new SecondPage());
		}
	}

	public class SecondPage : ContentPage
	{
		public SecondPage()
		{
			var label = new Label()
			{
				AutomationID = "SecondPageLabel",
				Text = "Welcome to Second Page",		
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			Content = label;
		}
	}
}

