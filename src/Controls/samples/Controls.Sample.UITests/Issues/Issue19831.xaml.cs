using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 19831, "[Android] Action mode menu doesn't disappear when switch on another tab", PlatformAffected.Android)]

public partial class Issue19831 : Shell
{
	public Issue19831()
	{
		InitializeComponent();
	}

	void Button_Clicked(System.Object sender, System.EventArgs e)
	{
		GoToAsync("//Page2");
	}
}
