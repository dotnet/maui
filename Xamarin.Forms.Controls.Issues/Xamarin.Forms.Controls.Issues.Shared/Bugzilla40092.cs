using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Threading.Tasks;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
    [Preserve(AllMembers = true)]
    [Issue(IssueTracker.Bugzilla, 40092, "Ensure android devices with fractional scale factors (3.5) don't have a white line around the border", PlatformAffected.Android)]
    public class Bugzilla40092 : TestContentPage
    {
        protected override void Init()
        {
            AbsoluteLayout mainLayout = new AbsoluteLayout()
            {
                BackgroundColor = Color.White
            };


            // The root page of your application
            var thePage = new ContentView
            {
                BackgroundColor = Color.Red,
                Content = mainLayout
            };

            BoxView view = new BoxView()
            {
                Color = Color.Black
            };

            mainLayout.Children.Add(view, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            Content = thePage;

        }
    }
}
