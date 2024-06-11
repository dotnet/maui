using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19556, "[Android] Systemfonts (light/black etc.) not working", PlatformAffected.Android)]

public partial class Issue19556 : ContentPage
{
	public Issue19556()
	{
		InitializeComponent();
	}
}
