namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19191, "Picker TitleColor not working", PlatformAffected.All)]
public partial class Issue19191 : ContentPage
{
	public Issue19191()
	{
		InitializeComponent();
	}
}
