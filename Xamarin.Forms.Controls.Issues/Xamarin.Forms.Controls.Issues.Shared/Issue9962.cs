using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9962, "NSException thrown when calling NSColor.ControlBackground.ToColor()", PlatformAffected.macOS)]
	public class Issue9962 : TestContentPage
	{
		public Issue9962()
		{
		}

		protected override void Init()
		{
			var stackLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			var debugLabel = new Label
			{
				Text = "The first button should show an alert with the exception message, second button should retrieve an actual color and put the value in this label and in the BoxView"
			};

			var boxView = new BoxView
			{
				BackgroundColor = Color.Blue,
				WidthRequest = 100,
				HeightRequest = 100
			};

			var buttonBoom = new Button
			{
				Text = "This button should throw an Exception"
			};

			buttonBoom.Clicked += (_, __) =>
			{
				try
				{
					var color = DependencyService.Get<INativeColorService>()?.GetConvertedColor(true);

					boxView.BackgroundColor = color ?? Color.Black;

				}
				catch (InvalidOperationException ex)
				{
					DisplayAlert("Exception!", ex.Message, "Gotcha");
				}
			};

			var buttonNotBoom = new Button
			{
				Text = "This button should NOT throw an Exception"
			};

			buttonNotBoom.Clicked += (_, __) =>
			{
				try
				{
					var color = DependencyService.Get<INativeColorService>()?.GetConvertedColor(false);

					debugLabel.Text = color?.ToString();
					boxView.BackgroundColor = color ?? Color.Black;
				}
				catch (Exception ex)
				{
					DisplayAlert("Exception!", ex.Message, "Gotcha");
				}
			};


			stackLayout.Children.Add(buttonBoom);
			stackLayout.Children.Add(buttonNotBoom);
			stackLayout.Children.Add(debugLabel);
			stackLayout.Children.Add(boxView);

			Content = stackLayout;
		}
	}
}