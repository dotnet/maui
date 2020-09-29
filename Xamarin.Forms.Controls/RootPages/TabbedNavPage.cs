using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	// TabbedPage -> NavigationPage
	public class TabbedNavPage : FlyoutPage
	{
		public TabbedNavPage()
		{
			NavigationPage navpage = null;
			navpage = new NavigationPage(new ContentPage
			{
				Title = "Content0",
				Content = new Button
				{
					Text = "Button",
					Command = new Command(() =>
					{
						Debug.WriteLine("k");
						navpage.PushAsync(new ContentPage() { Title = "Content1" });
					})
				}
			})
			{
				Title = "Page0",
			};

			//Children.add(navpage);
			//Children.add(new HomeButton ());
		}
	}
}