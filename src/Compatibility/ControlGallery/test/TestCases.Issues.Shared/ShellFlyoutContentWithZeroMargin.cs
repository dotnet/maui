using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Content With Zero Margin offsets correctly",
		   PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellFlyoutContentWithZeroMargin : TestShell
	{
		public ShellFlyoutContentWithZeroMargin()
		{
		}

		protected override void Init()
		{
			AddFlyoutItem(CreateContentPage(), "Item 1");
			FlyoutContent = new Label()
			{
				Text = "I should not be offset by the safe area",
				AutomationId = "FlyoutLabel",
				Margin = new Thickness(0)
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
						Text = "Open Flyout. Content should not obey safe area",
					}
				}
			};

			return new ContentPage()
			{
				Content = layout
			};
		}

#if UITEST && __IOS__
		[Test]
		public void FlyoutContentIgnoresSafeAreaWithZeroMargin()
		{
			RunningApp.WaitForElement("PageLoaded");
			this.ShowFlyout();
			var flyoutLoc = RunningApp.WaitForElement("FlyoutLabel")[0].Rect.Y;
			Assert.AreEqual(0, flyoutLoc);
		}
#endif
	}
}
