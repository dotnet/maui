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
	[Issue(IssueTracker.Bugzilla, 39829, "RowHeight of ListView is not working for UWP", PlatformAffected.WinRT)]
	public class Bugzilla39829 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Master";
			var listView = new ListView
			{
				RowHeight = 150,
				AutomationId = "listview",
				ItemsSource = new string[] { "Test1", "Test2", "Test3", "Test4", "Test5", "Test6", }
			};

			Content = listView;
		}

#if UITEST
		[Test]
		[Category("ManualReview")]
		public void Bugzilla39829Test()
		{
			RunningApp.WaitForElement(q => q.Marked("listview"));
			RunningApp.Screenshot("If there isn't substantial space between the list items, this test has failed.");
		}
#endif
	}
}
