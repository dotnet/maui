using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19081, "[iOS] RadioButton text related properties not working", PlatformAffected.iOS)]
	public partial class Issue19081 : ContentPage
	{
		public Issue19081()
		{
			InitializeComponent();
		}
	}

}