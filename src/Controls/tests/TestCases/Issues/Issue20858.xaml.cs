using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20858, "FlyoutPage open when rotating test", PlatformAffected.Android)]
	public partial class Issue20858 : FlyoutPage
	{
		public Issue20858()
		{
			InitializeComponent();
		}

		public void OpenFlyout_Clicked(object sender, EventArgs e)
		{
			IsPresented = true;
		}
	}
}