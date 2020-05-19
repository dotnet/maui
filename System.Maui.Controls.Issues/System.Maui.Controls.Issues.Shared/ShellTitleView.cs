using System.Maui.CustomAttributes;
using System.Maui.Internals;


#if UITEST
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Title View Test",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.TitleView)]
#endif
	public class ShellTitleView : TestShell
	{
		protected override void Init()
		{
			AddTopTab(createContentPage("title 1"), "page 1");
			AddTopTab(createContentPage(null), "page 2");
			AddTopTab(createContentPage("title 3"), "page 3");
			AddTopTab(createContentPage("title 4"), "page 4");

			ContentPage createContentPage(string titleView)
			{
				ContentPage page = new ContentPage()
				{
					Content = new StackLayout()
					{
						Children =
						{
							new Label()
							{
								Text = "Click through the tabs and make sure title view changes and doesn't duplicate"
							}
						}
					}
				};

				page.ToolbarItems.Add(new ToolbarItem() { IconImageSource = "coffee.png", Order = ToolbarItemOrder.Primary, Priority = 0 });

				if (!string.IsNullOrWhiteSpace(titleView))
				{
					Shell.SetTitleView(page,
						new StackLayout()
						{
							AutomationId = "TitleViewId",
							Children = { new Label() { Text = titleView } }
						});
				}

				return page;
			}
		}


#if UITEST && (__IOS__ || __ANDROID__)

		[Test]
		public void NoDuplicateTitleViews()
		{
			var titleView = RunningApp.WaitForElement("TitleViewId");

			Assert.AreEqual(1, titleView.Length);

			RunningApp.Tap("page 2");
			RunningApp.Tap("page 3");
			RunningApp.Tap("page 4");
			titleView = RunningApp.WaitForElement("TitleViewId");

			Assert.AreEqual(1, titleView.Length);
		}
#endif

	}
}
