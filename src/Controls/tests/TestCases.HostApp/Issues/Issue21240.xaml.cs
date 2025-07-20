using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 21240, "FlyoutPage IsGestureEnabled not working", PlatformAffected.Android)]

public partial class Issue21240 : FlyoutPage
{
	public Issue21240()
	{
		InitializeComponent();
	}
}
