using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Content Offsets Correctly",
		   PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class ShellFlyoutContentOffset : TestShell
	{
		public ShellFlyoutContentOffset()
		{
		}

		protected override void Init()
		{
			AddFlyoutItem(CreateContentPage(), "Item 1");
			FlyoutFooter = new Button()
			{
				HeightRequest = 200,
				AutomationId = "CloseFlyout",
				Command = new Command(() => FlyoutIsPresented = false),
				Text = "Close Flyout"
			};
		}

		ContentPage CreateContentPage()
		{
			var layout = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						AutomationId = "PageLoaded",
						Text = "Toggle through the 3 variations of flyout content and verify they all offset the same. Toggle the Header/Footer and then verify again",
					},
					new Button()
					{
						Text = "Toggle Flyout Content",
						Command = new Command(() =>
						{
							if (FlyoutContent is ScrollView)
								FlyoutContent = null;
							else if (FlyoutContent == null)
								FlyoutContent = new Label()
								{
									AutomationId = "LabelContent",
									Text = "Only Label"
								};
							else
								FlyoutContent = new ScrollView()
								{
									Content = new Label()
									{
										AutomationId = "ScrollViewContent",
										Text = "Label inside ScrollView"
									}
								};
						}),
						AutomationId = "ToggleFlyoutContent"
					},
					new Button()
					{
						Text = "Toggle Header",
						Command = new Command(() =>
						{
							if (FlyoutHeader == null)
							{
								FlyoutHeader =
									new BoxView()
									{
										BackgroundColor = Color.Blue,
										HeightRequest = 50
									};
							}
							else
							{
								FlyoutHeader = FlyoutFooter = null;
							}
						}),
						AutomationId = "ToggleHeader"
					}
				}
			};

			return new ContentPage()
			{
				Content = layout
			};
		}

#if UITEST
		[Test]
		public void FlyoutContentOffsetsCorrectly()
		{
			RunningApp.WaitForElement("PageLoaded");
			var flyoutLoc = GetLocationAndRotateToNextContent("Item 1");
			var labelLoc = GetLocationAndRotateToNextContent("LabelContent");
			var scrollViewLoc = GetLocationAndRotateToNextContent("ScrollViewContent");

			Assert.AreEqual(flyoutLoc, labelLoc, "Label Offset Incorrect");
			Assert.AreEqual(flyoutLoc, scrollViewLoc, "ScrollView Offset Incorrect");
		}

		[Test]
		public void FlyoutContentOffsetsCorrectlyWithHeader()
		{
			RunningApp.Tap("ToggleHeader");
			GetLocationAndRotateToNextContent("Item 1");
			var labelLoc = GetLocationAndRotateToNextContent("LabelContent");
			var scrollViewLoc = GetLocationAndRotateToNextContent("ScrollViewContent");

			Assert.AreEqual(labelLoc, scrollViewLoc, "ScrollView Offset Incorrect");
		}

		float GetLocationAndRotateToNextContent(string automationId)
		{
			ShowFlyout();
			var y = RunningApp.WaitForElement(automationId)[0].Rect.Y;
			RunningApp.Tap("CloseFlyout");
			RunningApp.Tap("ToggleFlyoutContent");

			return y;
		}
#endif
	}
}
