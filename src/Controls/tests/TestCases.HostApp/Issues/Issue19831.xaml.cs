using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19831, "[Android] Action mode menu doesn't disappear when switch on another tab", PlatformAffected.Android)]
public class Issue19831 : Issue19831Base
{
}

#if ANDROID
[Issue(IssueTracker.Github, 19831, "ShellRenderer title updates after switching tabs with context actions open", PlatformAffected.Android, issueTestNumber: 1)]
public class Issue19831Renderer : Issue19831Base
{
}
#endif

[XamlCompilation(XamlCompilationOptions.Compile)]
public abstract partial class Issue19831Base : Shell
{
	protected Issue19831Base()
	{
		InitializeComponent();
	}

	void Button_Clicked(System.Object sender, System.EventArgs e)
	{
		GoToAsync("//Page2");
	}
}
