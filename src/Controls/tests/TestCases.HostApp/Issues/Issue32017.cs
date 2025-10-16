using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32017, "Image shifts downward when window is resized smaller", PlatformAffected.UWP)]
	public class Issue32017 : TestContentPage
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout
			{
				Margin = new Thickness(20)
			};

			var titleLabel = new Label
			{
				Text = "My Recipes",
				FontSize = 24,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 0, 0, 20)
			};

			// Recreate the CarouselView structure from MyRecipesPage.xaml
			var carouselView = new CarouselView
			{
				WidthRequest = 350,
				HeightRequest = 570,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				AutomationId = "RecipeCarousel"
			};

			// Create sample recipe data
			var recipes = new[]
			{
				new { ImageUrl = "dotnet_bot.png", Name = "Recipe 1" },
				new { ImageUrl = "dotnet_bot.png", Name = "Recipe 2" },
				new { ImageUrl = "dotnet_bot.png", Name = "Recipe 3" }
			};

			carouselView.ItemsSource = recipes;

			// Define the item template that matches MyRecipesPage.xaml
			carouselView.ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid();

				var image = new Image
				{
					AutomationId = "RecipeImage",
					Aspect = Aspect.AspectFill, // This is key, same as in MyRecipesPage.xaml
					VerticalOptions = LayoutOptions.Fill,
					HorizontalOptions = LayoutOptions.Fill
				};
				image.SetBinding(Image.SourceProperty, "ImageUrl");

				var nameLabel = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 18,
					TextColor = Colors.White,
					BackgroundColor = Color.FromArgb("#80000000"), // Semi-transparent background
					Padding = new Thickness(10)
				};
				nameLabel.SetBinding(Label.TextProperty, "Name");

				grid.Children.Add(image);
				grid.Children.Add(nameLabel);

				return grid;
			});

			var instructionLabel = new Label
			{
				Text = "Resize the window to a smaller size and observe if the image shifts downward. The fix should prevent image position jumping during resize.",
				FontSize = 14,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 20, 0, 0),
				AutomationId = "InstructionLabel"
			};

			var statusLabel = new Label
			{
				Text = "Status: Image container behavior should remain stable during window resize",
				FontSize = 12,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 10, 0, 0),
				AutomationId = "StatusLabel"
			};

			stackLayout.Children.Add(titleLabel);
			stackLayout.Children.Add(carouselView);
			stackLayout.Children.Add(instructionLabel);
			stackLayout.Children.Add(statusLabel);

			Content = stackLayout;
		}
	}
}