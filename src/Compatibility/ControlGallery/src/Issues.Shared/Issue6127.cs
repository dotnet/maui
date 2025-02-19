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
	[Issue(IssueTracker.Github, 6127, "[Bug] ToolbarItem Order property ignored",
		PlatformAffected.Android)]
	public class Issue6127 : TestShell
	{
		const string PrimaryToolbarIcon = "PrimaryToolbarIcon";
		const string SecondaryToolbarIcon = "SecondaryToolbarIcon";

		protected override void Init()
		{
			AddTopTab(createContentPage("title 1"), "page 1");

			ContentPage createContentPage(string titleView)
			{
				var page = new ContentPage
				{
					Content = new StackLayout
					{
						Children =
						{
							new Label
							{
								Text = "If there is one toolbar item visible and one under the overflow menu, this test passed"
							}
						}
					}
				};

				page.ToolbarItems.Add(new ToolbarItem { IconImageSource = "coffee.png", Order = ToolbarItemOrder.Primary, Priority = 0, AutomationId = PrimaryToolbarIcon });
				page.ToolbarItems.Add(new ToolbarItem { Text = "Coffee", IconImageSource = "coffee.png", Order = ToolbarItemOrder.Secondary, Priority = 0, AutomationId = SecondaryToolbarIcon });

				return page;
			}
		}

#if UITEST && __ANDROID__
		[Test]
		public void Issue6127Test() 
		{
			RunningApp.WaitForElement (PrimaryToolbarIcon);

			RunningApp.Tap("More options");
			RunningApp.WaitForElement (SecondaryToolbarIcon);

			RunningApp.Screenshot ("There is a secondary toolbar menu and item");
		}
#endif
	}
}