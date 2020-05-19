using System;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
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
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
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
