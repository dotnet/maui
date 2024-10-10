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
			PrintButtonTexts();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{

			if (sender is Button buttonClicked)
				buttonClicked.Text = String.Empty; // Changing this to any other string doesn't reproduce the problem

			PrintButtonTexts();
		}

		private void PrintButtonTexts()
		{
			ButtonText.Text = $"Button1.Text = '{Button1.Text}'{Environment.NewLine}"
				+ $"Button2.Text = '{Button2.Text}'{Environment.NewLine}";
		}
	}
}