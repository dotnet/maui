using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

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
			SelectedTabColor = Colors.Purple;
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

#if UITEST && WINDOWS
		[Test]
		public void Issue6323Test()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
