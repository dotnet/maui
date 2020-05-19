using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	[Issue (IssueTracker.Bugzilla, 32776, "MasterDetail page - page title not showing for Windows Phone 8.1 (any orientation) or in Windows 8.1 (portrait orientation).", PlatformAffected.WinRT, NavigationBehavior.PushModalAsync)]
    public class Bugzilla32776
		: MasterDetailPage
    {
		public Bugzilla32776 ()
		{
			Master = new ContentPage { Title = "Content page" };
			Detail = new NavigationPage (new ContentPage {
				Title = "Test"
			});
		}
    }
}
