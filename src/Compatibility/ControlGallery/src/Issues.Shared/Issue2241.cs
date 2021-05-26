using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2241, "ScrollView content can become stuck on orientation change (iOS)", PlatformAffected.iOS)]
	public class Issue2241 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.Red,
				HeightRequest = 400,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				AutomationId = "MainGrid"
			};
			grid.RowDefinitions.Add(new RowDefinition { Height = 10 });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = 10 });

			var boxView = new BoxView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Color = Colors.Yellow
			};
			Grid.SetRow(boxView, 0);

			var label = new Label
			{
				Text = "If the view is scrollable, scroll down to see the yellow line at the bottom of the red box. Scroll back to top. Rotate the device to landscape mode."
				+ " Scroll down to see the yellow line again. Rotate the device back to portrait while you are still looking at the yellow line at the bottom of the red box."
				+ " If the view was originally scrollable, it should still be scrollable. You should still be able to see both of the yellow lines.",
				LineBreakMode = LineBreakMode.WordWrap,
				Margin = new Thickness(10, 0),
				VerticalTextAlignment = TextAlignment.Center
			};
			Grid.SetRow(label, 1);

			var boxView2 = new BoxView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Color = Colors.Yellow
			};
			Grid.SetRow(boxView2, 2);

			grid.Children.Add(boxView);
			grid.Children.Add(label);
			grid.Children.Add(boxView2);

			var scrollView = new ScrollView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Padding = new Thickness(20),
				Content = grid
			};

			Content = scrollView;
		}

#if UITEST
		[Test]
		[Ignore("Fails intermittently on TestCloud")]
		public void ChangeOrientationCheckScroll ()
		{
#if __ANDROID__
			var isAndroid = true;
#else
			var isAndroid = false;
#endif
			var grid1 = RunningApp.Query("MainGrid").Single();
			RunningApp.SetOrientationLandscape ();
			RunningApp.ScrollDown ();
			RunningApp.SetOrientationPortrait ();
			var grid2 = RunningApp.Query("MainGrid").Single();
			RunningApp.Screenshot ("Did it resize ok? Do you see some white on the bottom?");

			if (!isAndroid) {
				Assert.AreEqual (grid1.Rect.CenterY, grid2.Rect.CenterY);
			}
		}
#endif
	}
}

