using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Maui.Controls.UITests;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19956, "Sticky headers and bottom content insets", PlatformAffected.iOS)]
	public partial class Issue19956 : ContentPage
	{
		public Issue19956()
		{
			InitializeComponent();
		}
	}
}
