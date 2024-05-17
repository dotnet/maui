using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Issue(IssueTracker.Github, 12714,
		"[Bug] iOS application suspended at UICollectionViewFlowLayout.PrepareLayout() when using IsVisible = false",
		PlatformAffected.iOS)]
	public class Issue12714 : TestContentPage
	{
		const string Success = "Success";
		const string Show = "Show";

		protected override void Init()
		{
			var items = new List<string>() { "uno", "dos", "tres", Success };
			var cv = new CollectionView
			{
				ItemsSource = items,
				IsVisible = false,
				ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
			};

			var layout = new StackLayout() { Margin = 40 };

			var button = new Button { Text = Show };
			button.Clicked += (sender, args) => { cv.IsVisible = !cv.IsVisible; };

			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void InitiallyInvisbleCollectionViewSurvivesiOSLayoutNonsense()
		{
			RunningApp.WaitForElement(Show);
			RunningApp.Tap(Show);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
