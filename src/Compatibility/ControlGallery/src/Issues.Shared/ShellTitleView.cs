using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Title View Test",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.TitleView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
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
				Label safeArea = new Label();
				ContentPage page = new ContentPage()
				{
					Content = new StackLayout()
					{
						Children =
						{
							new Label()
							{
								Text = "Tab 1,3, and 4 should have a single visible TitleView. If the TitleView is duplicated or not visible the test has failed.",
								AutomationId = "Instructions"
							},
							safeArea
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


#if UITEST

		[Test]
		public void TitleViewPositionsCorrectly()
		{
			var titleView = RunningApp.WaitForElement("TitleViewId")[0].Rect;
			var topTab = RunningApp.WaitForElement("page 2")[0].Rect;

			var titleViewBottom = titleView.Y + titleView.Height;
			var topTabTop = topTab.Y;

			Assert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned behind tabs");
		}

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
