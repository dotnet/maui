using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1763, "First item of grouped ListView not firing .ItemTapped", PlatformAffected.WinPhone, NavigationBehavior.PushAsync)]
	public class Issue1763 : TestTabbedPage
	{
		public Issue1763 ()
		{
		
		}

		protected override void Init()
		{
			Title = "Contacts";
			Children.Add(new ContactsPage());
		}

#if UITEST
		[Test]
		public void TestIssue1763ItemTappedFiring()
		{
			RunningApp.WaitForElement(q => q.Marked("Contacts"));
			RunningApp.Tap(q => q.Marked("Egor1"));
			RunningApp.WaitForElement(q => q.Marked("Tapped a List item"));
			RunningApp.Tap(q => q.Marked("Destruction"));
			RunningApp.WaitForElement(q => q.Marked("Contacts"));
		}
#endif

	}
}
