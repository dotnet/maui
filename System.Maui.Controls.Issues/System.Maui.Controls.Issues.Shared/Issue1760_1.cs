using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1760, "Content set after an await is not visible", PlatformAffected.Android, issueTestNumber: 1)]
	public class Issue1760_1 : TestMasterDetailPage
	{
		protected override void Init()
		{
			Master = new Issue1760._1760Master(false);
			Detail = new Issue1760._1760TestPage(false);
			IsPresented = true;
		}

#if UITEST && __ANDROID__
		[Test]
		public void Issue1760_1Test()
		{
			RunningApp.WaitForElement(Issue1760.Before);
			RunningApp.WaitForElement(Issue1760.After);

			RunningApp.Tap("Test Page 1");
			RunningApp.WaitForElement(Issue1760.Before);
			RunningApp.WaitForElement(Issue1760.After);
		}
#endif
	}
}