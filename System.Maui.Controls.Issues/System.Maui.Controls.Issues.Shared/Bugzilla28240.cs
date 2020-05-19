using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 28240, "Problems with a NavigationPage as Master+Detail of a MasterDetailPage", PlatformAffected.Android)]
	public class Bugzilla28240 : TestMasterDetailPage
	{
		protected override void Init ()
		{
			Detail = new NavigationPage( new ContentPage { Title = "DetailPage", BackgroundColor = Color.Red });
			Master = new NavigationPage( new ContentPage { Title = "MasterPage", BackgroundColor = Color.Blue }) { Title =" Master" };
		}

		protected override async void OnAppearing()
		{
			var btn = new Button() { Text = "GO Back" };
			btn.Clicked += async (object sender, EventArgs e) => await (Master as NavigationPage).PopAsync();

			await (Master as NavigationPage).PushAsync(new ContentPage { Title = "New MasterPage", Content = btn, BackgroundColor = Color.Pink });
			base.OnAppearing();
		}
	}
}
