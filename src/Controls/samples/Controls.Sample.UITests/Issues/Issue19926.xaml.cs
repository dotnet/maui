using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19926, "[Android] Opacity bug on BoxView.Background", PlatformAffected.Android)]

public partial class Issue19926 : ContentPage
{
	public Issue19926()
	{
		InitializeComponent();
	}
}
