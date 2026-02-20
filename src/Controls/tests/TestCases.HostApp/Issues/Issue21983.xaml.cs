using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21983, "GradientBrushes are not supported on Shape.Stroke", PlatformAffected.All)]

public partial class Issue21983 : ContentPage
{
	public Issue21983()
	{
		InitializeComponent();
	}
}
