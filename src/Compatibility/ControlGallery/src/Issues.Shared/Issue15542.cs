using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 15542, "[Bug] Shell.TitleView does not render on iOS 16",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.TitleView)]
#endif
	public class Issue15542 : TestShell
	{
		protected override void Init()
		{
			AddTopTab(createContentPage("title 1"), "page 1");
			AddTopTab(createContentPage("title 2"), "page 2");
			AddTopTab(createContentPage("title 3"), "page 3");

			static ContentPage createContentPage(string titleView)
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
								Text = "If the TitleView is not visible the test has failed.",
								AutomationId = "Instructions"
							},
							safeArea
						}
					}
				};

				if (!string.IsNullOrWhiteSpace(titleView))
				{
					SetTitleView(page,
						new Grid()
						{
							BackgroundColor = Colors.Red,
							AutomationId = "TitleViewId",
							Children = { new Label() { Text = titleView, VerticalTextAlignment = TextAlignment.End } }
						});
				}

				return page;
			}
		}


#if UITEST

		[Test]
		public void TitleViewHeightDoesntOverflow()
		{
			var titleView = RunningApp.WaitForElement("TitleViewId")[0].Rect;
			var topTab = RunningApp.WaitForElement("page 1")[0].Rect;

			var titleViewBottom = titleView.Y + titleView.Height;
			var topTabTop = topTab.Y;

			Assert.GreaterOrEqual(topTabTop, titleViewBottom, "Title View is incorrectly positioned in iOS 16");
		}
#endif

	}
}