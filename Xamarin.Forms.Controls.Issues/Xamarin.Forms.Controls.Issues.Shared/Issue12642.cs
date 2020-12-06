using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12642, "[iOS] Rapid ShellContent Navigation Causes Blank Screens",
		PlatformAffected.iOS)]
#if UITEST
	[Category(Core.UITests.UITestCategories.Github10000)]
	[Category(UITestCategories.Shell)]
#endif
	public class Issue12642 : TestShell
	{
		protected override void Init()
		{
			var page = AddTopTab("Tab 1");
			var page2 = AddTopTab("Tab 2");

			page.Content = CreateContent();
			page2.Content = CreateContent();

			StackLayout CreateContent()
			{
				return new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Click quickly between the tabs. If you stop clicking and the content is blank then the test has failed.",
							AutomationId = "Success"
						},
						new Button()
						{
							Text = "Run Test Automated",
							AutomationId = "AutomatedRun",
							Command = new Command(async () =>
							{
								for(int i = 0; i < 20; i++)
								{
									this.CurrentItem = Items[0].Items[0].Items[0];
									await Task.Delay(10);
									this.CurrentItem = Items[0].Items[0].Items[1];
									await Task.Delay(10);
								}
							})
						}
					}
				};
			}
		}

#if UITEST
		[Test]
		public void ClickingQuicklyBetweenTopTabsBreaksContent()
		{
			RunningApp.Tap("AutomatedRun");
			RunningApp.Tap("Success");
			RunningApp.Tap("AutomatedRun");
			RunningApp.Tap("Success");
		}
#endif
	}
}
