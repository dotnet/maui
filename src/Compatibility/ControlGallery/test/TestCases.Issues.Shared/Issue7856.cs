using System;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 7856,
		"[Bug]  Shell BackButtonBehaviour TextOverride breaks back",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue7856 : TestShell
	{

		const string ContentPageTitle = "Item1";
		const string ButtonId = "ButtonId";

		protected override void Init()
		{
			CreateContentPage(ContentPageTitle).Content =
				new StackLayout
				{
					Children =
					{
						new Button
						{
							AutomationId = ButtonId,
							Text = "Tap to Navigate To the Page With BackButtonBehavior",
							Command = new Command(NavToBackButtonBehaviorPage)
						}
					}
				};
		}

		private void NavToBackButtonBehaviorPage()
		{
			_ = Shell.Current.Navigation.PushAsync(new Issue7856_1());
		}

#if UITEST && __IOS__
		[Test]
		public void BackButtonBehaviorTest()
		{
			RunningApp.Tap(x => x.Text("Tap to Navigate To the Page With BackButtonBehavior"));

			RunningApp.WaitForElement(x => x.Text("Navigate again"));

			RunningApp.Tap(x => x.Text("Navigate again"));

			RunningApp.WaitForElement(x => x.Text("Test"));

			RunningApp.Tap(x => x.Text("Test"));

		}
#endif
	}
}
