using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20166, "Custom FlyoutIcon visible although FlyoutBehavior set to disabled", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue20166 : Shell
	{
		public Issue20166()
		{
			InitializeComponent();
		}

		void Button1_Clicked(System.Object sender, System.EventArgs e)
		{
			CurrentItem = shellTab2;
			Current.FlyoutIcon = String.Empty;
		}

	}
}