using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1384, "Image is grid issue", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue1384 : ContentPage
	{
		public Issue1384()
		{
			var grid = new Grid { BackgroundColor = Color.Red, VerticalOptions = LayoutOptions.Start };
			grid.Children.Add(new Image { Source = "photo.jpg", Aspect = Aspect.AspectFit });
			grid.Children.Add(new Label
			{
				Opacity = .75,
				VerticalTextAlignment = TextAlignment.Start,
				HorizontalTextAlignment = TextAlignment.End,
				Text = "top and flush right",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.End,
				HeightRequest = 30,
				TextColor = Color.White
			});
			grid.Children.Add(new Label
			{
				Opacity = .75,
				VerticalTextAlignment = TextAlignment.End,
				Text = "bottom and flush left",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				VerticalOptions = LayoutOptions.End,
				HeightRequest = 40,
				TextColor = Color.White,
				BackgroundColor = Color.Green,
			});
			Content = grid;
		}
	}
}

