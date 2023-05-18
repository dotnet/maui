using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42074, "[Android] Clicking cancel on a TimePicker does not cause it to unfocus", PlatformAffected.Android)]
	public class Bugzilla42074 : TestContentPage
	{
		const string TimePicker = "TimePicker";

		protected override void Init()
		{
			var timePicker = new TimePicker
			{
				AutomationId = TimePicker
			};
			var timePickerFocusButton = new Button
			{
				Text = "Click to focus TimePicker",
				Command = new Command(() => timePicker.Focus())
			};
			Content = new StackLayout
			{
				Children =
				{
					timePicker,
					timePickerFocusButton
				}
			};
		}

#if UITEST

#if __ANDROID__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
        [Test]
        public void TimePickerCancelShouldUnfocus()
        {
            RunningApp.Tap(q => q.Marked(TimePicker));

			RunningApp.Back();
            RunningApp.WaitForElement(q => q.Marked("Click to focus TimePicker"));

            RunningApp.Tap(q => q.Marked("Click to focus TimePicker"));
			RunningApp.Back();
        }
#endif

#endif
	}
}
