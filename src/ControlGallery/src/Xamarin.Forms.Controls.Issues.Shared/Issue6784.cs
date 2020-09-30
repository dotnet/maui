using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using System.Linq;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6784, "ShellItem.CurrentItem is not set when selecting shell section aggregated in more tab", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue6784 : TestShell
	{
		protected override void Init()
		{
			var shellItem = new ShellItem()
			{
				Title = "ShellItem"
			};

			Items.Add(shellItem);

			AddBottomTab("Tab 1");
			AddBottomTab("Tab 2");
			AddBottomTab("Tab 3");
			AddBottomTab("Tab 4").AutomationId = "Tab 4 Content";
			var contentPage5 = AddBottomTab("Tab 5");
			AddBottomTab("Tab 6");

			shellItem.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == CurrentItemProperty.PropertyName)
				{
					if (((ShellItem)sender).CurrentItem.AutomationId == "Tab 5")
					{
						contentPage5.Content = new Label()
						{
							Text = "Success"
						};
					}
				}
			};
		}

#if UITEST && __IOS__
		[Test]
		public void CurrentItemIsSetWhenSelectingShellSectionAggregatedInMoreTab()
		{
			RunningApp.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
			RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));

			RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));
			RunningApp.Tap(x => x.Class("UITableViewCell").Text("Tab 5"));

			RunningApp.WaitForElement(x => x.Text("Success"));
		}

		[Test]
		public void MoreControllerOpensOnFirstClick()
		{
			RunningApp.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
			RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));

			RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));
			RunningApp.Tap(x => x.Class("UITableViewCell").Text("Tab 5"));

			RunningApp.Tap(x => x.Class("UITabBarButton").Marked("Tab 4"));
			RunningApp.WaitForElement("Tab 4 Content");

			RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));
			RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 6"));
		}

		[Test]
		public void MoreControllerDoesNotShowEditButton()
		{
			RunningApp.WaitForElement(x => x.Class("UITabBarButton").Marked("More"));
			RunningApp.Tap(x => x.Class("UITabBarButton").Marked("More"));

			RunningApp.WaitForElement(x => x.Class("UITableViewCell").Text("Tab 5"));

			Assert.AreEqual(RunningApp.Query(x => x.Marked("Edit")).Count(), 0);
		}
#endif
	}
}