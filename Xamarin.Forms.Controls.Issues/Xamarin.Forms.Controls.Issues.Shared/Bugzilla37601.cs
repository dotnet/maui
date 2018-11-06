using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
    [Preserve (AllMembers = true)]
    [Issue (IssueTracker.Bugzilla, 37601, "ToolbarItem throws error when navigating to TabbedPage ",
        PlatformAffected.WinPhone)]
    public class Bugzilla37601 : TestNavigationPage
    {
        protected override void Init ()
        {
            Navigation.PushAsync (new SelectPage ());
        }
    }

    internal class SelectPage : ContentPage
    {
        public SelectPage ()
        {
            var button = new Button { Text = "Move" };

            var label = new Label {
                Text =
                    "Click the Move button. If the next page is displayed, the test has passed. If the app crashes, the test has failed."
            };

            Content = new StackLayout {
                Children = { label, button }
            };

            button.Clicked += (sender, args) => { Navigation.PushAsync (new TabbedMain (), true); };

            ToolbarItems.Add (new ToolbarItem { Text = "Log Out" });
        }
    }

    internal class TabbedMain : TabbedPage
    {
        public TabbedMain ()
        {
            var page1 = new ContentPage { Title = "Page1" };
            page1.Content = new StackLayout {
                Children = { new Label { Text = "If you can see this, we haven't crashed. Yay!" } }
            };

           Children.Add (page1);
           Children.Add (new ContentPage { Title = "Page2" });
        }
    }
}
