using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21059, "[iOS] Disabled Editors Show Keyboard Partially When Tapped", PlatformAffected.iOS)]

public partial class Issue21059 : ContentPage
{
	public Issue21059()
	{
		InitializeComponent();
	}
}
