using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6323, "TabbedPage Page not watching icon changes", PlatformAffected.UWP)]
	public class Issue6323 : TestTabbedPage
	{
		protected override void Init()
		{
			SelectedTabColor = Color.Purple;
			Device.StartTimer(TimeSpan.FromSeconds(2), () =>
			{
				Children.Add(new ContentPage
				{
					Content = new Label { Text = "Success" },
					Title = "I'm a title"
				});
				return false;
			});
		}

#if UITEST && __WINDOWS__
		[Test]
		public void Issue6323Test()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
