using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38770, "RaiseChild and LowerChild do not work on Windows", PlatformAffected.WinRT)]
	public class Bugzilla38770 : TestContentPage
	{
		protected override void Init()
		{
			var red = new BoxView
			{
				BackgroundColor = Color.Red,
				WidthRequest = 50,
				HeightRequest = 50,
				TranslationX = 25
			};
			var green = new BoxView
			{
				BackgroundColor = Color.Green,
				WidthRequest = 50,
				HeightRequest = 50
			};
			var blue = new BoxView
			{
				BackgroundColor = Color.Blue,
				WidthRequest = 50,
				HeightRequest = 50,
				TranslationX = -25
			};
			var boxStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 0,
				Margin = new Thickness(0,50,0,0),
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					red,
					green,
					blue
				}
			};

			var raiseButtons = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Raise Red",
						WidthRequest = 110,
						Command = new Command(() => boxStack.RaiseChild(red))
					},
					new Button
					{
						Text = "Raise Green",
						WidthRequest = 110,
						Command = new Command(() => boxStack.RaiseChild(green))
					},
					new Button
					{
						Text = "Raise Blue",
						WidthRequest = 110,
						Command = new Command(() => boxStack.RaiseChild(blue))
					}
				}
			};
			var lowerButtons = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Lower Red",
						WidthRequest = 110,
						Command = new Command(() => boxStack.LowerChild(red))
					},
					new Button
					{
						Text = "Lower Green",
						WidthRequest = 110,
						Command = new Command(() => boxStack.LowerChild(green))
					},
					new Button
					{
						Text = "Lower Blue",
						WidthRequest = 110,
						Command = new Command(() => boxStack.LowerChild(blue))
					}
				}
			};

			Content = new StackLayout
			{
				Children =
				{
					raiseButtons,
					lowerButtons,
					boxStack
				}
			};
		}
	}
}