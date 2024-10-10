using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 18235, "[7.096] Setting .NET MAUI Button.Text to String.Empty inside a Clicked event handler causes previously set buttons to revert to previous values", PlatformAffected.iOS)]
	public partial class Issue18235 : ContentPage
	{
		public Issue18235()
		{
			InitializeComponent();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			if (sender is Button buttonClicked)
				buttonClicked.Text = String.Empty;
		}
	}
}