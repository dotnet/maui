using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8988, "App freezes on iPadOS 13.3 when in split view mode (multi-tasking) and change between pages", PlatformAffected.iOS)]
	public class Issue8988 : TestContentPage // or TestFlyoutPage, etc ...
	{
		SecondPage secondPage;

		protected override void Init()
		{
			secondPage = new SecondPage();
			var layout = new StackLayout();
			var label = new Label
			{
				Text = "Click Next to push a modal, pop it, and push it again, you should see the second page a 2nd time without glitches.",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				HorizontalTextAlignment = TextAlignment.Center
			};
			var button = new Button
			{
				Text = "NextPage",
				Command = new Command(async () =>
				{
					await Navigation.PushModalAsync(secondPage, false);
				})

			};

			layout.Children.Add(label);
			layout.Children.Add(button);

			BackgroundColor = Colors.YellowGreen;
			Content = layout;
		}

		[Preserve(AllMembers = true)]
		class SecondPage : ContentPage
		{
			public SecondPage()
			{
				var layout = new StackLayout();
				var label = new Label
				{
					Text = "This is the Second Page! Pop me and push again. I should look the same",
					VerticalOptions = LayoutOptions.CenterAndExpand,
					HorizontalOptions = LayoutOptions.CenterAndExpand,
					HorizontalTextAlignment = TextAlignment.Center
				};
				var button = new Button
				{
					Text = "Go Back to Main Page",
					Command = new Command(() => Navigation.PopModalAsync(false))
				};

				layout.Children.Add(label);
				layout.Children.Add(button);

				BackgroundColor = Colors.Yellow;

				Content = layout;
			}

			protected override void LayoutChildren(double x, double y, double width, double height)
			{
				base.LayoutChildren(x, y, width, height);
			}

			protected override void InvalidateMeasure()
			{
				base.InvalidateMeasure();
			}
		}
	}
}