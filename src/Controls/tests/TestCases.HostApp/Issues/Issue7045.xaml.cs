namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 7045, "Shell BackButtonBehavior binding Command to valid ICommand causes back button to disappear", PlatformAffected.All)]
public partial class Issue7045 : Shell
{
	public Command BackCommand { get; }

	public Issue7045()
	{
		InitializeComponent();
		NavigateButton.Clicked += async (s, e) => await Current.GoToAsync($"//DetialPage"); ;
		DetailPage.BindingContext = this;
	}
}