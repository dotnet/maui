using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19465, "Double tap gesture NullReferenceException when navigating", PlatformAffected.Android)]
	public class Issue19465 : NavigationPage
	{
		public Issue19465() : base(new Issue19465Content())
		{
		}

		public class Issue19465Content : ContentPage
		{
			public Issue19465Content()
			{
				var layout = new StackLayout();

				var button = new Button
				{
					AutomationId = "FirstButton",
					Text = "Navigate"
				};

				button.Clicked += OnNavigateClicked;

				layout.Children.Add(button);
				Content = layout;
			}

			async void OnNavigateClicked(object sender, System.EventArgs e)
			{
				await Navigation.PushAsync(new Issue19465SecondPage());
			}
		}
	}

	internal class Issue19465SecondPage : ContentPage
	{
		public Issue19465SecondPage()
		{
			var layout = new StackLayout();
			var tapGestureRecognizer = new TapGestureRecognizer();
			tapGestureRecognizer.Tapped += OnTapGestureRecognizerTapped;
			tapGestureRecognizer.NumberOfTapsRequired = 2;

			layout.GestureRecognizers.Add(tapGestureRecognizer);
			var label = new Label
			{
				AutomationId = "SecondLabel",
				Text = "Double Tap"
			};

			layout.Children.Add(label);
			Content = layout;
		}

		async void OnTapGestureRecognizerTapped(object sender, TappedEventArgs e)
		{
			Navigation.InsertPageBefore(new Issue19465ThirdPage(), this);
			await Navigation.PopAsync(animated: false);
		}
	}

	internal class Issue19465ThirdPage : ContentPage
	{
		public Issue19465ThirdPage()
		{
			var layout = new StackLayout();

			var button = new Button
			{
				AutomationId = "ThirdButton",
				Text = "Navigate Back"
			};

			button.Clicked += OnNavigateClicked;

			layout.Children.Add(button);
			Content = layout;
		}

		async void OnNavigateClicked(object sender, System.EventArgs e)
		{
			await Navigation.PopAsync();
		}
	}
}