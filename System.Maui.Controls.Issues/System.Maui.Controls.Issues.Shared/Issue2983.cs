using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue(IssueTracker.Github, 2983, "ListView.Footer can cause NullReferenceException", PlatformAffected.iOS)]
	public class Issue2983 : TestContentPage
	{
		protected override void Init ()
		{
			Content = new ListView {
				Footer = new StackLayout {
					Children = {new Label {Text = "Footer", AutomationId = "footer"}}
				}
			};
		}

#if UITEST
		[Test]
		public void TestDoesNotCrash ()
		{
			RunningApp.WaitForElement (c => c.Marked ("footer"));
		}
#endif
	}
}