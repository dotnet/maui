using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1384, "Image is grid issue", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue1384:ContentPage
	{
		public Issue1384 ()
		{
			var grid = new Grid {BackgroundColor = Color.Red, VerticalOptions=LayoutOptions.Start};
			grid.Children.Add (new Image {Source = "photo.jpg", Aspect = Aspect.AspectFit});
			grid.Children.Add (new Label { 
				Opacity =.75,
#pragma warning disable 618
				YAlign = TextAlignment.Start,
#pragma warning restore 618

#pragma warning disable 618
				XAlign = TextAlignment.End,
#pragma warning restore 618
				Text ="top and flush right",
#pragma warning disable 618
				Font = Font.SystemFontOfSize (NamedSize.Large),
#pragma warning restore 618
				VerticalOptions=LayoutOptions.Start,
				HorizontalOptions=LayoutOptions.End,
				HeightRequest=30,
				TextColor = Color.White
			});
			grid.Children.Add (new Label { 
				Opacity =.75,
#pragma warning disable 618
				YAlign = TextAlignment.End,
#pragma warning restore 618
				Text ="bottom and flush left",
#pragma warning disable 618
				Font = Font.SystemFontOfSize (NamedSize.Large),
#pragma warning restore 618
				VerticalOptions=LayoutOptions.End,
				HeightRequest=40,
				TextColor = Color.White,
				BackgroundColor = Color.Green,
			});
			Content = grid;
		}
	}
}

