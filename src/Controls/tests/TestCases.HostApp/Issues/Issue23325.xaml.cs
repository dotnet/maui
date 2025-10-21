using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23325, "Setting background color on the Searchbar does nothing", PlatformAffected.All)]
	public partial class Issue23325 : Shell
	{
		public Issue23325()
		{
			InitializeComponent();
		}
	}
}