using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2354, "ListView, ImageCell and disabled source cache and same image url", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue2354 : TestContentPage
	{
		protected override void Init()
		{
			var presidents = new List<President>();

			presidents.Add(new President($"Presidente 44", 1, $"https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/Fruits.jpg?raw=true"));
			presidents.Add(new President($"Presidente 43", 2, $"https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/person.png?raw=true"));
			presidents.Add(new President($"Presidente 42", 3, $"https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/photo.jpg?raw=true"));
			presidents.Add(new President($"Presidente 41", 4, $"https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/FlowerBuds.jpg?raw=true"));
			presidents.Add(new President($"Presidente 40", 5, $"https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/games.png?raw=true"));
			presidents.Add(new President($"Presidente 39", 6, $"https://github.com/xamarin/Xamarin.Forms/blob/17881ec93d6b3fb0ee5e1a2be46d7eeadef23529/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/gear.png?raw=true"));
			presidents.Add(new President($"Presidente 38", 7, $"https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/xamarinlogo.png?raw=true"));
			presidents.Add(new President($"Presidente 37", 8, $"https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/xamarinstore.jpg?raw=true"));
			presidents.Add(new President($"Presidente 36", 9, $"https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/oasis.jpg?raw=true"));
			presidents.Add(new President($"Presidente 35", 10, $"https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/Vegetables.jpg?raw=true"));

			var header = new Label
			{
				Text = "Presidents",
				HorizontalOptions = LayoutOptions.Center
			};

			var cell = new DataTemplate(typeof(CustomCell));

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				AutomationId = "TestListView",
				ItemsSource = presidents,
				ItemTemplate = cell,
				RowHeight = 200
			};


			Content = new StackLayout
			{
				Children = {
					header,
					listView
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class President
		{
			public President(string name, int position, string image)
			{
				Name = name;
				Position = position;
				Image = image;
			}

			public string Name { private set; get; }

			public int Position { private set; get; }

			public string Image { private set; get; }
		}


		[Preserve(AllMembers = true)]
		public class CustomCell : ViewCell
		{
			public CustomCell()
			{
				var image = new Image
				{
					HorizontalOptions = LayoutOptions.Start,
					Aspect = Aspect.AspectFill,
					AutomationId = "ImageLoaded",
				};

				var source = new UriImageSource
				{
					CachingEnabled = false,
				};

				source.SetBinding(UriImageSource.UriProperty, new Binding("Image", converter: new UriConverter()));

				image.Source = source;


				View = image;
			}
		}

		[Preserve(AllMembers = true)]
		public class UriConverter : IValueConverter
		{

			public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				return new Uri((string)value);
			}

			public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}
	}
}