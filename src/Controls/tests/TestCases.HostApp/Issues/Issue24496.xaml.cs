using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 24496, "Pickers scroll to bottom and new keyboard types rekick the scrolling", PlatformAffected.iOS)]
	public partial class Issue24496 : ContentPage
	{
		public Issue24496()
		{
			InitializeComponent();
		}
	}
}