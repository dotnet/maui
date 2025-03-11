namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 25558, "ImageButton dosen't scale Image correctly", PlatformAffected.Android)]
public partial class Issue25558 : ContentPage
{
	public Issue25558()
	{
		InitializeComponent();
	}
}