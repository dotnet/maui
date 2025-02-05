using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22594, "[Android] Label.CharacterWrap wraps text too soon", PlatformAffected.Android)]

public partial class Issue22594 : ContentPage
{
	public Issue22594()
	{
		InitializeComponent();
	}
}