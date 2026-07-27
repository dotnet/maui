namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 32200, "NavigationPage TitleView iOS 26", PlatformAffected.iOS)]
public class Issue32200NavigationPage : NavigationPage
{
	public Issue32200NavigationPage() : base(new Issue32200()) { }
}
public partial class Issue32200 : ContentPage
{
	public Issue32200()
	{
		InitializeComponent();
	}
}