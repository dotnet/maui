namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 13474, "FontImageSource defaults and style", PlatformAffected.All)]
public partial class Issue13474 : ContentPage
{
	public Issue13474()
	{
		InitializeComponent();
	}
}