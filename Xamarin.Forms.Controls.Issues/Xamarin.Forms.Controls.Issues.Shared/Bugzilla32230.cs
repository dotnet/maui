using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32230, "isPresentedChanged raises multiple times")]
	public class Bugzilla32230 : TestMasterDetailPage // or TestMasterDetailPage, etc ...
	{
		Label _lblCount;
		Button _btnOpen;
		Button _btnClose;
		int _count;

		protected override void Init ()
		{
			_lblCount = new Label { Text = _count.ToString (), AutomationId = "lblCount" };
			_btnOpen = new Button { Text = "Open", AutomationId = "btnOpen", 
				Command = new Command (() => {
					IsPresented = true;
				})
			};
			_btnClose = new Button { Text = "Close", AutomationId = "btnClose", 
				Command = new Command (() => {
					IsPresented = false;
				})
			};

			Master = new ContentPage {
				Title = "Master",
				Content = new StackLayout { Children = { _lblCount, _btnClose } }
			};

			Detail = new NavigationPage (new ContentPage { Content = _btnOpen });
			IsPresentedChanged += (object sender, EventArgs e) => {
				_count++;
				_lblCount.Text = _count.ToString();
			};
		}

#if UITEST
		[Test]
		public void Bugzilla32230Test ()
		{
			RunningApp.Tap (q => q.Marked ("btnOpen"));
			RunningApp.WaitForElement("1");
			RunningApp.Tap (q => q.Marked ("btnClose"));
			RunningApp.Tap (q => q.Marked ("btnOpen"));
			RunningApp.WaitForElement("3");
		}
#endif
	}
}
