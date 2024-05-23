using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12777, "[Bug] CarouselView NRE if item template is not specified",
		PlatformAffected.iOS)]
	public class Issue12777 : TestContentPage
	{
		public Issue12777()
		{
			BindingContext = new Issue12777ViewModel();
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = new Thickness(12),
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Without exceptions, the test has passed."
			};

			var carouselView = new CarouselView
			{
				AutomationId = "TestCarouselView"
			};

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue12777ViewModel.Items));

			layout.Children.Add(instructions);
			layout.Children.Add(carouselView);

			Content = layout;
		}
	}
}