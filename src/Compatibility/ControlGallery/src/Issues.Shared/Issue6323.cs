//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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
