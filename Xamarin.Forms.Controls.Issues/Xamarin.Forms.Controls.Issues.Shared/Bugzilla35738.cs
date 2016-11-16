using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
    [Preserve (AllMembers = true)]
    public class CustomButton : Button
    {
        // In the Android project, there's a custom renderer set up for this type
    }

    [Preserve (AllMembers = true)]
    [Issue (IssueTracker.Bugzilla, 35738, "ButtonRenderer UpdateTextColor function crash", PlatformAffected.Android)]
    public class Bugzilla35738 : TestContentPage
    {
        protected override void Init ()
        {
            var label = new Label () { Text = "If you can see the button, this test has passed" };
            var customButton = new CustomButton () { Text = "This is a custom button", TextColor = Color.Fuchsia };

            Content = new StackLayout () {
                Children = { label, customButton }
            };
        }

#if UITEST
        [Test]
        [UiTest (typeof(TestContentPage))]
        public void CallingOnElementChangedOnCustomButtonShouldNotCrash ()
        {
            RunningApp.WaitForElement (q => q.Marked ("This is a custom button"));
        }
#endif
    }
}