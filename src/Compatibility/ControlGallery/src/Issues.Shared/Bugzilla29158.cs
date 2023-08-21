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
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29158, "XF for WP8.1RT - BeginInvokeOnMainThread generates NullReferenceException", (PlatformAffected)(1 << 3))]
	public class Bugzilla29158
		: ContentPage
	{
		protected override void OnAppearing()
		{
			base.OnAppearing();

			System.Threading.Tasks.Task.Run(async () =>
		   {
			   await System.Threading.Tasks.Task.Delay(1000);
			   Device.BeginInvokeOnMainThread(() => DisplayAlert("Time's up", "", "OK"));
		   });
		}
	}
}
