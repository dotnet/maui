using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1219, "Setting ToolbarItems in ContentPage constructor crashes app", PlatformAffected.iOS)]
	public class Issue1219 : ContentPage
	{
		public Issue1219 ()
		{
			ToolbarItems.Add(new ToolbarItem ("MenuItem", "", () => {

			}));
		}
	}
}

