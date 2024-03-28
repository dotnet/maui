using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 18843, "[Android] Wrong left margin in the navigation bar", PlatformAffected.Android)]

public partial class Issue18843 : ContentPage
{
	public Issue18843()
	{
		InitializeComponent();
	}
}
