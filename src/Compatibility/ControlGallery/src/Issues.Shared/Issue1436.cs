using System.Reflection;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1436, "Button border not drawn on Android without a BorderRadius", PlatformAffected.Android)]
	public class Issue1436 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid
			{
				ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
				RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) } }
			};

			grid.Children.Add(new Button
			{
				Text = "Button",
				HorizontalOptions = LayoutOptions.End,
				BorderColor = Colors.AliceBlue,
				BorderWidth = 5,
				AutomationId = "TestReady"
			}, 0, 0);

			grid.Children.Add(new Button
			{
				Text = "Button",
				HorizontalOptions = LayoutOptions.Start,
				BackgroundColor = Colors.Gray
			}, 1, 1);

			grid.Children.Add(new Button
			{
				Text = "Button",
				HorizontalOptions = LayoutOptions.End,
			}, 0, 1);

			grid.Children.Add(new Button
			{
				Text = "Button",

				HorizontalOptions = LayoutOptions.Start,
			}, 1, 0);

			StackLayout stackLayout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label{ Text = "The following four buttons should all be the same size. One has a ridiculous border. One has a darker background. One is the default button." },
					grid,
					new Label{ Text = "The following three buttons should all have a red border." },
					new Button {
						Text = "BorderWidth = 1, CornerRadius = [default],",
						HorizontalOptions = LayoutOptions.Center,
						BorderColor = Colors.Red,
						BorderWidth = 1,
					},
					new Button {
						Text = "BorderWidth = 1, CornerRadius = 0",
						HorizontalOptions = LayoutOptions.Center,
						BackgroundColor = Colors.Blue,
						BorderColor = Colors.Red,
						BorderWidth = 1,
						CornerRadius = 0,
						TextColor = Colors.White
					},
					new Button {
						Text = "BorderWidth = 1, CornerRadius = 1",
						HorizontalOptions = LayoutOptions.Center,
						BackgroundColor = Colors.Black,
						BorderColor = Colors.Red,
						BorderWidth = 1,
						CornerRadius = 1,
						TextColor = Colors.White
					}
				},
			};

			foreach (var element in stackLayout.Descendants())
			{
				//TODO: if (element is Button button)
				var button = element as Button;
				if (button != null)
					button.On<Android>().SetUseDefaultPadding(true).SetUseDefaultShadow(true);
			}

			Content = stackLayout;
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Issue1436Test()
		{
			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("I am at Issue 1436");
		}
#endif
	}
}
