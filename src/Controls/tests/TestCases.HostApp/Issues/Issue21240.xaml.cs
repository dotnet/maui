using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21240, "FlyoutPage IsGestureEnabled not working on Android", PlatformAffected.Android)]
public partial class Issue21240 : FlyoutPage
{
	public Issue21240()
	{
		InitializeComponent();
		IsPresented = false;
	}
}
