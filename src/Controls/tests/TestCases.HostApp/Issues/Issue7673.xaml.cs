namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 7673, "Android Switch view has inconsistent colors when off", PlatformAffected.All)]
public partial class Issue7673 : ContentPage
{
	public Issue7673()
	{
		InitializeComponent();
	}
}