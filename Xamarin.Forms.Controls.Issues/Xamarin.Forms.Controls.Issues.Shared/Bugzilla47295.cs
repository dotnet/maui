using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
    [Preserve(AllMembers = true)]
    [Issue(IssueTracker.Bugzilla, 47295, "Toolbar in a NavigationPage is clipped when expanded", PlatformAffected.WinRT)]
    public class Bugzilla47295 : TestNavigationPage
    {
        protected override void Init()
        {
            PushAsync(new TestPage47295());
        }
    }

    [Preserve(AllMembers = true)]
    public class TestPage47295 : ContentPage
    {
        public TestPage47295()
        {
            var descLabel = new Label { Text = "Press '...' to expand the CommandBar. The ToolbarItem's text label should not be clipped." };
            var pressedLabel = new Label { Text = "ToolbarItem pressed.", IsVisible = false };
            Content = new StackLayout
            {
                Children = {
                    descLabel,
                    pressedLabel
                }
            };

            ToolbarItems.Add(new ToolbarItem("Test", "toolbar_close", () => pressedLabel.IsVisible = true));
        }
    }
}