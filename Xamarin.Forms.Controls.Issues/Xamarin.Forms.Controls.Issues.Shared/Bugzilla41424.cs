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
	[Issue(IssueTracker.Bugzilla, 41424, "[Android] Clicking cancel on a DatePicker does not cause it to unfocus", PlatformAffected.Android)]
	public class Bugzilla41424 : TestContentPage
	{
		const string DatePicker = "DatePicker";

		protected override void Init()
		{
			var datePicker = new DatePicker
			{
				AutomationId = DatePicker
			};
			var datePickerFocusButton = new Button
			{
				Text = "Click to focus DatePicker",
				Command = new Command(() => datePicker.Focus())
			};
			Content = new StackLayout
			{
				Children =
				{
					datePicker,
					datePickerFocusButton
				}
			};
		}

#if UITEST

#if __ANDROID__
		[Test]
		public void DatePickerCancelShouldUnfocus()
		{
			RunningApp.Tap(q => q.Marked(DatePicker));
					
			RunningApp.Back();
			RunningApp.WaitForElement(q => q.Marked("Click to focus DatePicker"));

			RunningApp.Tap(q => q.Marked("Click to focus DatePicker"));
			RunningApp.Back();
		}
#endif
		
#endif
	}
}
