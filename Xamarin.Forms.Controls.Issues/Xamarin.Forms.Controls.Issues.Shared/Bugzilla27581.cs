using System;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Bugzilla, 27581, "WebView in ContentPage", PlatformAffected.Android)]
	public class Bugzilla27581 : ContentPage
	{
		public Bugzilla27581 ()
		{
			Content = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = {
					new Label {
						XAlign = TextAlignment.Center,
						Text = "Tap input field, then try to scroll"
					},
					new WebView {
						Source = "http://movinglabs.com/temp/xamarin/input.html",
						VerticalOptions = LayoutOptions.FillAndExpand
					}
				}
			};
		}
	}
}