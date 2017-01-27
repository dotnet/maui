using System;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest.Android;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2272, "Entry text updating set focus on the beginning of text not the end of it", PlatformAffected.Android)]
	public class Issue2272 : TestContentPage
	{
		protected override void Init ()
		{
			var userNameEditor = new Entry () { Text = "userNameEditorEmptyString" };
			userNameEditor.Focused += (sender, args) => {
				userNameEditor.Text = "focused";
			};

			Content = new StackLayout {
				Spacing = 10,
				VerticalOptions = LayoutOptions.Start,
				Children = { userNameEditor }
			};
		}

#if UITEST
		[Test]
#if __MACOS__
		[Ignore("EnterText problems in UITest Desktop")]
#endif
		public void TestFocusIsOnTheEndAfterSettingText ()
		{
			RunningApp.Tap (c => c.Marked ("userNameEditorEmptyString"));
			RunningApp.EnterText ("1");
			PressEnter ();
			Assert.Greater (RunningApp.Query (c => c.Marked ("focused1")).Length, 0);
		}

		void PressEnter ()
		{
			var androidApp = RunningApp as AndroidApp;
			if (androidApp != null) {
				androidApp.PressUserAction (UserAction.Done);
			}
			else {
				RunningApp.PressEnter ();
			}
		}
#endif
	}
}

