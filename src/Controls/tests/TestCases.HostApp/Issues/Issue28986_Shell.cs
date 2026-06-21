#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea Shell Page for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 6)]
public partial class Issue28986_Shell : Shell
{
	public Issue28986_Shell() : base()
    {
        var page = new Issue28986_ContentPage();
        page.ToolbarItems.Add(new ToolbarItem { Text = "Item 1" });
        page.Title = "SafeArea Shell Test";
        Shell.SetBackgroundColor(page, Colors.Blue);
        Items.Add(new FlyoutItem()
        {
            Items =
            {
                new ShellContent()
                {
                    Content = page,
                }
            }
        });

        Items.Add(new FlyoutItem()
        {
            Items =
            {
                new ShellContent()
                {
                    Content = new ContentPage()
                    {
                        Title = "Page 2"
                    },
                }
            }
        });

#if ANDROID
		// Set SoftInput.AdjustNothing - we have full control over insets (iOS-like behavior)
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
    }
}