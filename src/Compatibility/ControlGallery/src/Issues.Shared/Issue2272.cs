using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest.Android;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2272, "Entry text updating set focus on the beginning of text not the end of it", PlatformAffected.Android)]
	public class Issue2272 : TestContentPage
	{
		protected override void Init()
		{
			var userNameEditor = new Entry() { AutomationId = "userNameEditorEmptyString", Text = "userNameEditorEmptyString" };
			userNameEditor.Focused += (sender, args) =>
			{
				userNameEditor.Text = "focused";
			};

			Content = new StackLayout
			{
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
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TestFocusIsOnTheEndAfterSettingText ()
		{
			RunningApp.WaitForElement("userNameEditorEmptyString");
			RunningApp.Tap (c => c.Marked ("userNameEditorEmptyString"));
			RunningApp.EnterText ("1");
			PressEnter ();
			var q = RunningApp.Query(c => c.Marked("userNameEditorEmptyString"));
			Assert.AreEqual("focused1", q[0].Text);
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

