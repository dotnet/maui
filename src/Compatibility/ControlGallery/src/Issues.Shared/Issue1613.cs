using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1613, "Map.GetSizeRequest always returns map's current size", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue1613 : ContentPage
	{
		public Issue1613()
		{
			Build();
		}

		async void Build()
		{
			var image = new Image
			{
				Source = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/main/banner.png",
				Aspect = Aspect.AspectFill,
				Opacity = 0.5,
			};

			var name = new Label
			{
				Text = "Foo",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = 30,
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.White,
			};

			var nameView = new AbsoluteLayout
			{
				HeightRequest = 170,
				BackgroundColor = Color.Black,
				Children = {
					{image, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All},
					{name, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All}
				},
			};

			var addressLabel = new Label
			{
				Text = "Loading address…",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
			};

			var map = new Map
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
			};

			Content = new StackLayout
			{
				Children = { nameView, addressLabel, map },
			};

			await Task.Delay(1000);
			addressLabel.Text = "Updated with new\nmultiline\nlabel";
		}
	}
}
