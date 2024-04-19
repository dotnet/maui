using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21630, "Entries in NavBar don't trigger keyboard scroll", PlatformAffected.iOS)]
public partial class Issue21630 : ContentPage
{
	public Issue21630()
	{
		InitializeComponent();
	}
}
