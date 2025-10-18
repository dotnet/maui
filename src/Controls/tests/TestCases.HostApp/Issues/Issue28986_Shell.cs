using System.Drawing;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea Shell Page for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 6)]
public partial class Issue28986_Shell : Shell
{
	public Issue28986_Shell() : base()
    {
        var page = new Issue28986_ContentPage();
        Shell.SetBackgroundColor(page, Colors.Blue);
        CurrentItem = page;
    }
}