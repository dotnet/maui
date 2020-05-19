using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1613, "Map.GetSizeRequest always returns map's current size", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue1613 : ContentPage
	{
		public Issue1613 ()
		{
			Build ();
		}

		async void Build ()
		{
			var image = new Image {
				Source = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/banner.png",
				Aspect = Aspect.AspectFill,
				Opacity = 0.5,
			};

			var name = new Label {
				Text = "Foo",
#pragma warning disable 618
				XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
				YAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
				Font = Font.SystemFontOfSize(30, FontAttributes.Bold),
#pragma warning restore 618
				TextColor = Color.White,
			};

			var nameView = new AbsoluteLayout {
				HeightRequest = 170,
				BackgroundColor = Color.Black,
				Children = { 
					{image, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All},  
					{name, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All} 
				},
			};
				
			var addressLabel = new Label {
				Text = "Loading address…",
#pragma warning disable 618
				XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
				YAlign = TextAlignment.Center,
#pragma warning restore 618
			};
										
			var map = new Map {
				VerticalOptions = LayoutOptions.FillAndExpand,
			};

			Content = new StackLayout {
				Children = { nameView, addressLabel, map },
			};

			await Task.Delay (1000);
			addressLabel.Text = "Updated with new\nmultiline\nlabel";
		}
	}
}
