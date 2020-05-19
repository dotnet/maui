using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0,"NavigationPage push pop", PlatformAffected.All, NavigationBehavior.PushModalAsync)]
	public class NavPage : ContentPage
	{
		public NavPage ()
		{
			NavigationPage page = null;

			var popButton1 = new Button () { Text = "Pop", BackgroundColor = Color.Blue };
			popButton1.Clicked += (s, a) => Navigation.PopModalAsync ();

			page = new NavigationPage (new ContentPage { Content = popButton1 });
			Navigation.PushModalAsync (page);
		}
	}
}
