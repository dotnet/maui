using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
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
	[Issue(IssueTracker.Github, 11244, "[Bug] BackButtonBehavior no longer displays on the first routed page in 4.7",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue11244 : TestShell
	{
		protected async override void Init()
		{
			var page1 = AddContentPage<TabBar, Tab>();
			ContentPage page = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "The app bar should have text instead of a hamburger"
						},
						new Button()
						{
							Text = "Run test again",
							Command = new Command(async () =>
							{
								CurrentItem = page1;
								await Task.Delay(1000);
								await GoToAsync("//MainPage");
							})
						}
					}
				}
			};

			Shell.SetBackButtonBehavior(page,
				new BackButtonBehavior()
				{
					TextOverride = "Logout",
					Command = new Command(() => { })
				});

			var page2 = AddContentPage<TabBar, Tab>(page);
			page2.Route = "MainPage";
			await Task.Delay(1000);
			await GoToAsync("//MainPage");
		}


#if UITEST && __SHELL__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void LeftToolbarItemTextDisplaysWhenFlyoutIsDisabled()
		{
			RunningApp.WaitForElement("Logout");
		}
#endif
	}
}
