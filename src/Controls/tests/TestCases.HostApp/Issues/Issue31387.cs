using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31387, "[Android] CarouselView incorrectly reads out \"double tap to activate\"", PlatformAffected.Android)]
	public class Issue31387 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Enable TalkBack on Android. Navigate to the CarouselView below. " +
					   "TalkBack should NOT announce 'double tap to activate' since CarouselView does not support item selection.",
				AutomationId = "Instructions",
				Margin = 10
			};

			var carousel = new CarouselView
			{
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" },
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						FontSize = 24
					};
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
				HeightRequest = 200,
				AutomationId = "TestCarouselView"
			};

			Content = new StackLayout
			{
				Children = { instructions, carousel }
			};
		}
	}
}
