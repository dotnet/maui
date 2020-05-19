using System;

using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1698, "ActionSheet extra buttons are not accessible", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1698 : ContentPage
	{
		public Issue1698 ()
		{
			Button btn = new Button
			{
				Text = "Display ActionSheet"
			};

			btn.Clicked += btn_Clicked;

			Content = btn;
		}

		public async void btn_Clicked(object sender, EventArgs e)
		{
			await DisplayActionSheet("Action Sheet", "Cancel", null, new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "Eleven", "twelve", "thirteen", "fourteen" });
		}
	}
}

