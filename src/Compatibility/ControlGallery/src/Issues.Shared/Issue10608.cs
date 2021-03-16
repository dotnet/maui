using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10608, "[Bug] [Shell] [iOS] Locked flyout causes application to freezes when quickly switching between tabs", PlatformAffected.iOS)]
	public class Issue10608 : TestShell
	{
		public Issue10608()
		{
		}

		void AddPage(string title)
		{
			var page = CreateContentPage<FlyoutItem>(title);

			page.Content = new Grid()
			{
				Children =
				{
					new ScrollView()
					{
						Content =
							new StackLayout()
							{
								Children =
								{
									new Button()
									{
										Text = "Learn More",
										Margin = new Thickness(0,10,0,0),
										BackgroundColor = Color.Purple,
										TextColor = Color.White,
										AutomationId = "LearnMoreButton"
									}
								}
							}
					}
				}
			};
		}

		protected override void Init()
		{
			FlyoutBehavior = FlyoutBehavior.Locked;

			AddPage("Click");
			AddPage("Between");
			AddPage("These Flyouts");
			AddPage("Really Fast");
			AddPage("If it doesn't");
			AddPage("Lock test has passed");

			int i = 0;
			foreach (var item in Items)
			{
				item.Items[0].AutomationId = $"FlyoutItem{i}";
				item.Items[0].Items.Add(new ContentPage()
				{
					Title = "Page"
				});

				i++;
			}

			Items.Add(new MenuItem()
			{
				Text = "Let me click for you",
				AutomationId = $"FlyoutItem{i}",
				Command = new Command(async () =>
				{
					for (int j = 0; j < 5; j++)
					{
						CurrentItem = Items[0].Items[0];
						await Task.Delay(10);
						CurrentItem = Items[1].Items[0];
						await Task.Delay(10);
					}

					CurrentItem = Items[0].Items[0];
				})
			});

			Items[0].Items[0].Items[0].Title = "Tab 1";
			Items[0].Items[0].Items[0].AutomationId = "Tab1AutomationId";
			Items[1].Items[0].Items[0].Title = "Tab 2";
			Items[1].Items[0].Items[0].AutomationId = "Tab2AutomationId";

			Items[0].FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
			Items[1].FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
		}

#if UITEST && __SHELL__
		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellWithTopTabsFreezesWhenNavigatingFlyoutItems()
		{
			RunningApp.Tap("FlyoutItem6");
			RunningApp.Tap("FlyoutItem0");
			for (int i = 0; i < 5; i++)
			{
				RunningApp.WaitForElement("Tab1AutomationId");
				RunningApp.WaitForElement("LearnMoreButton");
				RunningApp.Tap("FlyoutItem0");
				RunningApp.Tap("FlyoutItem1");
				RunningApp.Tap("FlyoutItem0");
				RunningApp.WaitForElement("LearnMoreButton");
			}

			RunningApp.WaitForElement("Tab1AutomationId");
			RunningApp.WaitForElement("LearnMoreButton");
			RunningApp.Tap("FlyoutItem1");
			RunningApp.WaitForElement("Tab2AutomationId");
			RunningApp.WaitForElement("LearnMoreButton");
			RunningApp.Tap("FlyoutItem0");
			RunningApp.WaitForElement("Tab1AutomationId");
			RunningApp.WaitForElement("LearnMoreButton");
		}
#endif
	}
}
