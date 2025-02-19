using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class ImageGallery : ContentPage
	{
		Image _autoPlayGif;

		public ImageGallery()
		{

			Padding = new Thickness(20);

			var normal = new Image { Source = ImageSource.FromFile("cover1.jpg") };
			var disabled = new Image { Source = ImageSource.FromFile("cover1.jpg") };
			var rotate = new Image { Source = ImageSource.FromFile("cover1.jpg") };
			var transparent = new Image { Source = ImageSource.FromFile("cover1.jpg") };
			var embedded = new Image { Source = ImageSource.FromResource("Microsoft.Maui.Controls.ControlGallery.GalleryPages.crimson.jpg", typeof(ImageGallery).Assembly) };
			var gif = new Image { Source = "GifOne.gif" };
			_autoPlayGif = new Image { Source = "GifOne.gif" };

			// let the stack shrink the images
			normal.MinimumHeightRequest = normal.MinimumHeightRequest = 10;
			disabled.MinimumHeightRequest = disabled.MinimumHeightRequest = 10;
			rotate.MinimumHeightRequest = rotate.MinimumHeightRequest = 10;
			transparent.MinimumHeightRequest = transparent.MinimumHeightRequest = 10;
			embedded.MinimumHeightRequest = 10;

			disabled.IsEnabled = false;
			rotate.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(o => rotate.RelRotateTo(180)) });
			transparent.Opacity = .5;

			Content =
				new ScrollView()
				{
					Content = new StackLayout
					{
						Orientation = StackOrientation.Horizontal,
						Children =
						{
							new StackLayout
							{
								//MinimumWidthRequest = 20,
								HorizontalOptions = LayoutOptions.FillAndExpand,
								Children =
								{
									new Label(){ Text = "Normal"},
									normal,
									new Label(){ Text = "Disabled"},
									disabled,
									new Label(){ Text = "Transparent"},
									transparent,
									new Label(){ Text = "Rotate"},
									rotate,
									new Label(){ Text = "Embedded"},
									embedded,
									new Label(){ Text = "Gif"},
									embedded,
									new Label(){ Text = "Autoplay Gif"},
									_autoPlayGif,
									new Label(){ Text = "Horizontal"},
									new StackLayout
									{
										HeightRequest = 30,
										Orientation = StackOrientation.Horizontal,
										Children =
										{
											new Image {Source = "cover1.jpg"},
											new Image {Source = "cover1.jpg"},
											new Image {Source = "cover1.jpg"},
											new Image {Source = "cover1.jpg"}
										}
									}
								}
							},
							new StackLayout
							{
								WidthRequest = 30,
								Children =
								{
									new Image {Source = "cover1.jpg"},
									new Image {Source = "cover1.jpg"},
									new Image {Source = "cover1.jpg"},
									new Image {Source = "cover1.jpg"}
								}
							}

						}
					}
				};
		}


		protected override void OnAppearing()
		{
			base.OnAppearing();
			_autoPlayGif.IsAnimationPlaying = true;
		}
	}
}
