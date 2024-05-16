using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22433, "Button Device Tests", PlatformAffected.All)]
public partial class Issue22433 : ContentPage
{
	public Issue22433()
	{
		InitializeComponent();
	}
}