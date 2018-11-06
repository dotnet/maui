using System;
using System.Diagnostics;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 229, "ToolbarItems broken", PlatformAffected.Android)]
	public class Issue229 : ContentPage
	{
		public Issue229 ()
		{
			Title = "I am a navigation page.";

			var label = new Label {
				Text = "I should have a toolbar item",
#pragma warning disable 618
				XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
				YAlign = TextAlignment.Center
#pragma warning restore 618
			};

			var refreshBtn = new ToolbarItem ("Refresh", null, () => label.Text = "Clicking it works");

			ToolbarItems.Add (refreshBtn);

			Content = label;
		}
	}
}
