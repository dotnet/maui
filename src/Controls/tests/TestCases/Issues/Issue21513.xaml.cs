using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21513, "Buttons with images don't cover text", PlatformAffected.UWP)]
public partial class Issue21513 : ContentPage
{
	public Issue21513()
	{
		InitializeComponent();
	}
}