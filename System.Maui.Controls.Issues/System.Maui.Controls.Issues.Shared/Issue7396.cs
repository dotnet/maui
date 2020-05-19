using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Linq;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7396, "Setting Shell.BackgroundColor overrides all colors of TabBar",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class Issue7396 : TestShell
	{
		const string CreateTopTabButton = "CreateTopTabButton";
		const string CreateBottomTabButton = "CreateBottomTabButton";
		const string ChangeShellColorButton = "ChangeShellBackgroundColorButton";

		protected override void Init()
		{
			var page = CreateContentPage();
			page.Title = "Main";
			page.Content = CreateEntryInsetView();

			CurrentItem = Items.Last();
		}

		View CreateEntryInsetView()
		{
			var random = new Random();
			ScrollView view = null;
			view = new ScrollView()
			{
				Content = new StackLayout()
				{
					Children =
						{
							new Button()
							{
								Text = "Top Tab",
								AutomationId = CreateTopTabButton,
								Command = new Command(() => AddTopTab("top"))
							},
							new Button()
							{
								Text = "Bottom Tab",
								AutomationId = CreateBottomTabButton,
								Command = new Command(() => AddBottomTab("bottom", "coffee.png"))
							},
							new Button()
							{
								Text = "Random Shell Background Color",
								AutomationId = ChangeShellColorButton,
								Command = new Command(() =>
									Shell.SetBackgroundColor(this, Color.FromRgb(random.Next(0,255), random.Next(0,255), random.Next(0,255))))
							},
						}
				}
			};

			return view;
		}

#if UITEST && __ANDROID__

		[Test]
		public void BottomTabColorTest()
		{
			//7396 Issue | Shell: Setting Shell.BackgroundColor overrides all colors of TabBar
			RunningApp.WaitForElement(CreateBottomTabButton);
			RunningApp.Tap(CreateBottomTabButton);
			RunningApp.Tap(CreateBottomTabButton);
			RunningApp.Tap(ChangeShellColorButton);
			RunningApp.Screenshot("I should see a bottom tabbar icon");
			Assert.Inconclusive("Check that bottom tabbar icon is visible");
		}
#endif
	}
}
