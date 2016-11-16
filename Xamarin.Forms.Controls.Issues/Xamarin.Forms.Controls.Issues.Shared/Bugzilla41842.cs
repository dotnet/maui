using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41842, "Set MasterDetailPage.Detail = New Page() twice will crash the application when set MasterBehavior = MasterBehavior.Split", PlatformAffected.WinRT)]
	public class Bugzilla41842 : TestMasterDetailPage
	{
		protected override void Init()
		{
			MasterBehavior = MasterBehavior.Split;

			Master = new Page() { Title = "Master" };

			Detail = new NavigationPage(new Page());
			Detail = new NavigationPage(new ContentPage { Content = new Label { Text = "Success" } });
		}

#if UITEST
		[Test]
		public void Bugzilla41842Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}
