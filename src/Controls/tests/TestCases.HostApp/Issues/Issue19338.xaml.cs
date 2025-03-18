namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19338, "Border.Shadow hide the collectionView Header", PlatformAffected.iOS)]
public partial class Issue19338 : ContentPage
{
	public Issue19338()
	{
		InitializeComponent();
	}
}