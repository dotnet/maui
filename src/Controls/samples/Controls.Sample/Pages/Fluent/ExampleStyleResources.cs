using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.Fluent
{
	public class ExampleStyleResources
	{
		public static readonly Color PrimaryColor = Color.FromArgb("512BD4");
		
		public static ResourceDictionary Default
			=> new ResourceDictionary
		{
			new Style(typeof(Button))
			{
				Button.TextColorProperty.Set(PrimaryColor),
				Button.BackgroundColorProperty.Set(Colors.White),
				Button.FontFamilyProperty.Set("OpenSansRegular"),
				Button.FontSizeProperty.Set(20),
				Button.CornerRadiusProperty.Set(8),
				Button.PaddingProperty.Set(new Thickness(14,10)),
				Button.MinimumHeightRequestProperty.Set(44),
				Button.MinimumWidthRequestProperty.Set(44)
			}
		};
	}
}

