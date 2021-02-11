using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Search Handler Item Sizing",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class ShellSearchHandlerItemSizing : TestShell
	{
		protected override void Init()
		{
			ContentPage contentPage = new ContentPage();
			AddFlyoutItem(contentPage, "Main Page");

			Shell.SetSearchHandler(contentPage, new TestSearchHandler()
			{
				AutomationId = "SearchHandler"
			});

			contentPage.Content =
				new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Type into the search handler to display a list. Each item should be measured to the size of the content"
						}
					}
				};
		}


		public class TestSearchHandler : SearchHandler
		{
			public TestSearchHandler()
			{
				ShowsResults = true;
				ItemsSource = Enumerable.Range(0, 100)
					.Select(_ => "searchresult")
					.ToList();
			}
		}

#if UITEST

		[Test]
		public void SearchHandlerSizesCorrectly()
		{
			RunningApp.WaitForElement("SearchHandler");
			RunningApp.EnterText("SearchHandler", "Hello");
			var contentSize = RunningApp.WaitForElement("searchresult")[0].Rect;
			Assert.Less(contentSize.Height, 100);
		}
#endif
	}
}
