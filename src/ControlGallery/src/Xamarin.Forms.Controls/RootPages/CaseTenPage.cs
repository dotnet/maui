using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	// May not behave
	// NavigationPage with multiple tabbed pages	
	public class CaseTenPage : FlyoutPage
	{
		public CaseTenPage()
		{
			var btn = new Button { Text = "Click Me" };
			btn.Clicked += (sender, args) => btn.Navigation.PushModalAsync(new NavigationPage(new ContentPage()));

			var detail = new ContentPage { Content = btn };

			NavigationPage.SetHasNavigationBar(detail, false);

			Flyout = new ListPage() { Title = "Flyout" };
			Detail = detail;
		}
	}
}