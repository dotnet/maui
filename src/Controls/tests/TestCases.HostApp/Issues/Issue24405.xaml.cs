using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 24405, "Entry with right aligned text keeps text jumping to the left during editing", PlatformAffected.UWP)]
	public partial class Issue24405 : ContentPage
	{
		public Issue24405()
		{
			InitializeComponent();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			entry.Text = "Hello";
		}
	}
}