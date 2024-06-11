using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 5924, "TableView ViewCell vanishes after content is updated", PlatformAffected.Android)]
	public partial class Issue5924 : ContentPage
	{
		public Issue5924()
		{
			InitializeComponent();
		}
	}
}