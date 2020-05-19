using System;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44461, "ScrollToPosition.Center works differently on Android and iOS", PlatformAffected.iOS)]
	public class Bugzilla44461 : TestContentPage
	{
		const string BtnPrefix= "Button";

		protected override void Init()
		{
			var grid = new Grid
			{
				RowSpacing = 0,
			};

			var instructions = new Label
			{
				Text = @"Tap the first button (Button0). The button should be aligned with the left side of the screen "
				+ "and should not move. If it's not, the test failed."
			};

			grid.Children.Add(instructions);

			var scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Horizontal,
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Color.Yellow,
				HeightRequest = 50
			};
			grid.Children.Add(scrollView);

			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 20
			};

			for (var i = 0; i < 10; i++)
			{
				var button = new Button
				{
					Text = $"{BtnPrefix}{i}"
				};
				button.Clicked += (sender, args) =>
				{
					scrollView.ScrollToAsync(sender as Button, ScrollToPosition.Center, true);
				};

				stackLayout.Children.Add(button);
			}
			scrollView.Content = stackLayout;
			Content = grid;
		}

#if UITEST && __IOS__
		[Test]
		public void Bugzilla44461Test()
		{
			var positions = TapButton(0);
			Assert.AreEqual(positions.initialPosition.X, positions.finalPosition.X);
			Assert.LessOrEqual(positions.finalPosition.X, 1);
			RunningApp.Screenshot ("Button0 is aligned with the left side of the screen");
		}

		(AppRect initialPosition, AppRect finalPosition ) TapButton(int position)
		{
			var buttonText = $"{BtnPrefix}{position}";
			RunningApp.WaitForElement(q => q.Button(buttonText));
			var initialPosition = RunningApp.Query(buttonText)[0].Rect;
			RunningApp.Tap(q => q.Button(buttonText));
			var finalPosition = RunningApp.Query(buttonText)[0].Rect;
			return (initialPosition, finalPosition);
		}
#endif
	}
}
