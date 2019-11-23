using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8529,
		"[Bug] [Shell] iOS - BackButtonBehavior Command property binding throws InvalidCastException when using a custom command class that implements ICommand",
		PlatformAffected.iOS)]
	public class Issue8529 : TestShell
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
			_ = Shell.Current.Navigation.PushAsync(new Issue8529_1());
		}

#if UITEST && __SHELL__
		public void NavigateBack()
		{
#if __IOS__
			RunningApp.Tap(c => c.Marked("BackButtonImage"));
#else
			RunningApp.Tap(FlyoutIconAutomationId);
#endif
		}

		[Test]
		public void Issue8529ShellBackButtonBehaviorCommandPropertyCanUseICommand()
		{
			RunningApp.WaitForElement(ButtonId, "Timed out waiting for first page.");
			RunningApp.Tap(ButtonId);
			RunningApp.WaitForElement("LabelId", "Timed out waiting for the destination page.");
			NavigateBack();
			RunningApp.WaitForElement(ButtonId, "Timed out waiting to navigate back to the first page.");
		}
#endif
	}
}