using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 18161, "Toggling FlyoutLayoutBehavior on Android causes the app to crash", PlatformAffected.Android)]
	public partial class Issue18161 : FlyoutPage
	{
		public Issue18161()
		{
			InitializeComponent();
		}

		public void ToggleBehaviour_Clicked(object sender, EventArgs e)
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split 
				? FlyoutLayoutBehavior.Popover 
				: FlyoutLayoutBehavior.Split;
		}
	}
}