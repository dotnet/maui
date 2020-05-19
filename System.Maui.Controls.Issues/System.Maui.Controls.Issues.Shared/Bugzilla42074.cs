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
