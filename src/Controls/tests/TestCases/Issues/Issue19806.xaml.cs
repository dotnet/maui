using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19806, "Button doesn't respect LineBreakMode", PlatformAffected.All)]
	public partial class Issue19806 : ContentPage
	{
		public Issue19806()
		{
			InitializeComponent();
		}
	}
}