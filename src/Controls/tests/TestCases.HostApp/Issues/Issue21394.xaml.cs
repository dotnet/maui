using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21394, "Buttons with Images layouts", PlatformAffected.UWP)]
public partial class Issue21394 : ContentPage
{
	public Issue21394()
	{
		InitializeComponent();
	}
}