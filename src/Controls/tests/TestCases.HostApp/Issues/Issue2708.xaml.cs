namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 2708, "Prevent tabs from being removed during modal navigation", PlatformAffected.Android)]
	public partial class Issue2708 : TabbedPage
	{
		public Issue2708()
		{
			InitializeComponent();
		}

		async void OnOpenModalClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new Issue2708Modal());
		}
	}

	public class Issue2708Modal : ContentPage
	{
		public Issue2708Modal()
		{
			Title = "Modal Page";

			// Use a semi-transparent background so tabs can be verified as visible behind the modal
			BackgroundColor = Color.FromArgb("#80000000");

			var layout = new StackLayout
			{
				Padding = new Thickness(20),
				BackgroundColor = Colors.White,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 300,
				Children =
				{
					new Label 
					{ 
						Text = "This is a modal page", 
						FontSize = 18, 
						HorizontalOptions = LayoutOptions.Center 
					},
					new Label 
					{ 
						Text = "The tabs should still be visible behind this modal on Android", 
						FontSize = 14,
						HorizontalOptions = LayoutOptions.Center 
					},
					new Button 
					{ 
						Text = "Close Modal", 
						AutomationId = "CloseModalButton"
					}
				}
			};

			var closeButton = layout.Children.OfType<Button>().First();
			closeButton.Clicked += async (sender, e) =>
			{
				await Navigation.PopModalAsync();
			};

			Content = layout;
		}
	}
}