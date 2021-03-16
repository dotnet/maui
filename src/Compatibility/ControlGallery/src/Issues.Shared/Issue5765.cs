using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5765, "[Frame, CollectionView, Android]The Label.Text is invisible on Android if DataTemplate have frame as layout",
		PlatformAffected.Android)]
	class Issue5765 : TestNavigationPage
	{
		const string Target = "FirstLabel";

		protected override void Init()
		{
			PushAsync(CreateRoot());
		}

		Frame CreateFrame()
		{
			var frame = new Frame() { CornerRadius = 10, BackgroundColor = Color.SeaGreen };

			var flexLayout = new FlexLayout()
			{
				Direction = FlexDirection.Row,
				JustifyContent = FlexJustify.SpaceBetween,
				AlignItems = FlexAlignItems.Stretch
			};

			var label1 = new Label { Text = "First Label", AutomationId = Target, HeightRequest = 100 };
			var label2 = new Label { Text = "Second Label" };

			flexLayout.Children.Add(label1);
			flexLayout.Children.Add(label2);

			frame.Content = flexLayout;

			return frame;
		}

		Page CreateRoot()
		{
			var page = new ContentPage() { Title = "Issue5765" };

			var cv = new CollectionView();

			cv.ItemTemplate = new DataTemplate(() =>
			{
				return CreateFrame();
			});

			cv.ItemsSource = new List<string> { "one", "two", "three" };

			page.Content = cv;

			return page;
		}

#if UITEST
		[Test]
		public void FlexLayoutsInFramesShouldSizeCorrectly()
		{
			// If the first label is visible at all, then this has succeeded
			RunningApp.WaitForElement(Target);
		}
#endif
	}
}
