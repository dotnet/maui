using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	//Issue 465
	public class Issue465Page : TabbedPage 
	{
		public Issue465Page () 
		{
			var popModalButton = new Button {
				Text = "PopModalAsync",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			var splashPage = new ContentPage { Content = popModalButton };

			Navigation.PushModalAsync (splashPage);

			popModalButton.Clicked += (s, e) => Navigation.PopModalAsync ();
			
		}
	}	
}