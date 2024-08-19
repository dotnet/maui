using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21173, "[Android] Border with RoundRectangle Stroke does not correctly round corners of things within it", PlatformAffected.Android)]

public partial class Issue21173 : ContentPage
{
	public Issue21173()
	{
		InitializeComponent();
	}
}
