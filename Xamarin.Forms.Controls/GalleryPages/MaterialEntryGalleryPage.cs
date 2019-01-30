using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls.GalleryPages
{
	public class MaterialEntryGalleryPage : ContentPage
	{
		public MaterialEntryGalleryPage()
		{
			Visual = VisualMarker.Material;
			Content =
				new ScrollView()
				{

					Content = new StackLayout()
					{
						Children =
						{
							new Entry()
							{
								Text = "With Text",
								Placeholder="Placeholder",
							},
							new Entry()
							{
								Placeholder = "Placeholder"
							},
							new Entry()
							{
								Placeholder = "Green Placeholder",
								PlaceholderColor = Color.Green,
							},
							new Entry()
							{
								Placeholder = "Purple Placeholder",
								PlaceholderColor = Color.Purple,
							},
							new Entry()
							{
								Text = "Green TextColor",
								TextColor = Color.Green
							},
							new Entry()
							{
								Text = "Purple TextColor",
								TextColor = Color.Purple
							},
							new Entry()
							{
								Text = "With Text larger font",
								Placeholder="Placeholder",
								FontSize = 24
							},
							new Entry()
							{
								Text = "Yellow BackgroundColor",
								BackgroundColor = Color.Yellow
							},
							new Entry()
							{
								Text = "Cyan BackgroundColor",
								BackgroundColor = Color.Cyan
							},
						}
					}

				};
		}
	}
}
