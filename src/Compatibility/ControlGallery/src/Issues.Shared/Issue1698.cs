using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1698, "ActionSheet extra buttons are not accessible", PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1698 : ContentPage
	{
		public Issue1698()
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

