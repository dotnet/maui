using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23934, "RelativeLayout content disappears when it has a border with a border stroke", PlatformAffected.All)]
	public partial class Issue23934 : ContentPage
	{
		public Issue23934()
		{
			InitializeComponent();
		}
	}
}