namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21368, "Image AspectFill is not honored", PlatformAffected.Android)]
public partial class Issue21368 : ContentPage
{
	public Issue21368()
	{
		InitializeComponent();
	}
}