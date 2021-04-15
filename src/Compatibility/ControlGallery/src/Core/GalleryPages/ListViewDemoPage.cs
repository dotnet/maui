using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	internal class ListViewDemoPage : ContentPage
	{
		class Person
		{
			public Person(string name, DateTime birthday, Color favoriteColor)
			{
				Name = name;
				Birthday = birthday;
				FavoriteColor = favoriteColor;
			}

			public string Name { private set; get; }

			public DateTime Birthday { private set; get; }

			public Color FavoriteColor { private set; get; }
		};

		class MyDataTemplateSelector : DataTemplateSelector
		{
			DataTemplate _oddTemplate;
			DataTemplate _evenTemplate;

			public MyDataTemplateSelector()
			{
				_evenTemplate = new DataTemplate(() =>
				{
					// Create views with bindings for displaying each property.
					Label nameLabel = new Label();
					nameLabel.SetBinding(Label.TextProperty, "Name");

					Label birthdayLabel = new Label();
					birthdayLabel.SetBinding(Label.TextProperty,
											  new Binding("Birthday", BindingMode.OneWay,
														   null, null, "Born {0:d}"));

					BoxView boxView = new BoxView();
					boxView.SetBinding(BoxView.ColorProperty, "FavoriteColor");

					// Return an assembled ViewCell.
					var viewCell = new ViewCell
					{
						View = new StackLayout
						{
							Padding = new Thickness(0, 5),
							Orientation = StackOrientation.Horizontal,
							Children = {
								new Image {
									HeightRequest = 40,
									WidthRequest = 40,
									Source = new UriImageSource {
									    //											CacheValidity = TimeSpan.FromSeconds (10),
									    Uri = new Uri ("https://xamarin.com/content/images/pages/index/xamarin-studio-icon.png"),
									}
								},
								boxView,
								new StackLayout {
									VerticalOptions = LayoutOptions.Center,
									Spacing = 0,
									Children = {
										nameLabel,
										birthdayLabel
									}
								}
							}
						}
					};

					viewCell.ContextActions.Add(new MenuItem()
					{
						Text = "Even Action"
					});
					return viewCell;
				});

				_oddTemplate = new DataTemplate(() =>
				{
					// Create views with bindings for displaying each property.
					Label nameLabel = new Label();
					nameLabel.SetBinding(Label.TextProperty, "Name");

					Label birthdayLabel = new Label();
					birthdayLabel.SetBinding(Label.TextProperty,
											  new Binding("Birthday", BindingMode.OneWay,
														   null, null, "Born {0:d}"));

					BoxView boxView = new BoxView();
					boxView.SetBinding(BoxView.ColorProperty, "FavoriteColor");

					// Return an assembled ViewCell.
					var viewCell = new ViewCell
					{
						View = new StackLayout
						{
							Padding = new Thickness(0, 5),
							Orientation = StackOrientation.Horizontal,
							Children = {
								new Image {
									HeightRequest = 40,
									WidthRequest = 40,
									Source = new UriImageSource {
									    //											CacheValidity = TimeSpan.FromSeconds (10),
									    Uri = new Uri ("https://xamarin.com/content/images/pages/index/xamarin-studio-icon.png"),
									}
								},

								new StackLayout {
									VerticalOptions = LayoutOptions.Center,
									Spacing = 0,
									Children = {
										birthdayLabel,
										nameLabel,
									}
								},
								boxView,
							}
						}
					};
					viewCell.Tapped += ViewCell_Tapped;
					viewCell.ContextActions.Add(new MenuItem()
					{
						Text = "Odd Action"
					});
					return viewCell;
				});
			}

			void ViewCell_Tapped(object sender, EventArgs e)
			{
				Debug.WriteLine("Item tapped");
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				return ((Person)item).Birthday.Month % 2 == 0 ? _evenTemplate : _oddTemplate;
			}
		}

		public ListViewDemoPage()
		{
			Label header = new Label
			{
				Text = "ListView",
				FontAttributes = FontAttributes.Bold,
				FontSize = 50,
				HorizontalOptions = LayoutOptions.Center
			};

			// Define some data.
			List<Person> people = new List<Person>
			{
				new Person("Abigail", new DateTime(1975, 1, 15), Colors.Aqua),
				new Person("Bob", new DateTime(1976, 2, 20), Colors.Black),
				new Person("Cathy", new DateTime(1977, 3, 10), Colors.Blue),
#pragma warning disable 618
                new Person("David", new DateTime(1978, 4, 25), Colors.Fuchsia),
#pragma warning restore 618
                new Person("Eugenie", new DateTime(1979, 5, 5), Colors.Gray),
				new Person("Freddie", new DateTime(1980, 6, 30), Colors.Green),
				new Person("Greta", new DateTime(1981, 7, 15), Colors.Lime),
				new Person("Harold", new DateTime(1982, 8, 10), Colors.Maroon),
				new Person("Irene", new DateTime(1983, 9, 25), Colors.Navy),
				new Person("Jonathan", new DateTime(1984, 10, 10), Colors.Olive),
				new Person("Kathy", new DateTime(1985, 11, 20), Colors.Purple),
				new Person("Larry", new DateTime(1986, 12, 5), Colors.Red),
				new Person("Monica", new DateTime(1975, 1, 5), Colors.Silver),
				new Person("Nick", new DateTime(1976, 2, 10), Colors.Teal),
				new Person("Olive", new DateTime(1977, 3, 20), Colors.White),
				new Person("Pendleton", new DateTime(1978, 4, 10), Colors.Yellow),
				new Person("Queenie", new DateTime(1979, 5, 15), Colors.Aqua),
				new Person("Rob", new DateTime(1980, 6, 30), Colors.Blue),
#pragma warning disable 618
                new Person("Sally", new DateTime(1981, 7, 5), Colors.Fuchsia),
#pragma warning restore 618
                new Person("Timothy", new DateTime(1982, 8, 30), Colors.Green),
				new Person("Uma", new DateTime(1983, 9, 10), Colors.Lime),
				new Person("Victor", new DateTime(1984, 10, 20), Colors.Maroon),
				new Person("Wendy", new DateTime(1985, 11, 5), Colors.Navy),
				new Person("Xavier", new DateTime(1986, 12, 30), Colors.Olive),
				new Person("Yvonne", new DateTime(1987, 1, 10), Colors.Purple),
				new Person("Zachary", new DateTime(1988, 2, 5), Colors.Red)
			};
			List<Person> people2 = new List<Person>
			{
				new Person("Abigail", new DateTime(1975, 1, 15), Colors.Aqua),
				new Person("Bob", new DateTime(1976, 2, 20), Colors.Black),
				new Person("Cathy", new DateTime(1977, 3, 10), Colors.Blue),
#pragma warning disable 618
                new Person("David", new DateTime(1978, 4, 25), Colors.Fuchsia),
#pragma warning restore 618
                new Person("Eugenie", new DateTime(1979, 5, 5), Colors.Gray),
				new Person("Freddie", new DateTime(1980, 6, 30), Colors.Green),
				new Person("Greta", new DateTime(1981, 7, 15), Colors.Lime),
				new Person("Harold", new DateTime(1982, 8, 10), Colors.Maroon),
				new Person("Irene", new DateTime(1983, 9, 25), Colors.Navy),
				new Person("Jonathan", new DateTime(1984, 10, 10), Colors.Olive),
				new Person("Kathy", new DateTime(1985, 11, 20), Colors.Purple),
				new Person("Larry", new DateTime(1986, 12, 5), Colors.Red),
				new Person("Monica", new DateTime(1975, 1, 5), Colors.Silver),
				new Person("Nick", new DateTime(1976, 2, 10), Colors.Teal),
				new Person("Olive", new DateTime(1977, 3, 20), Colors.White),
				new Person("Pendleton", new DateTime(1978, 4, 10), Colors.Yellow),
				new Person("Queenie", new DateTime(1979, 5, 15), Colors.Aqua),
				new Person("Rob", new DateTime(1980, 6, 30), Colors.Blue),
#pragma warning disable 618
                new Person("Sally", new DateTime(1981, 7, 5), Colors.Fuchsia),
#pragma warning restore 618
                new Person("Timothy", new DateTime(1982, 8, 30), Colors.Green),
				new Person("Uma", new DateTime(1983, 9, 10), Colors.Lime),
				new Person("Victor", new DateTime(1984, 10, 20), Colors.Maroon),
				new Person("Wendy", new DateTime(1985, 11, 5), Colors.Navy),
				new Person("Xavier", new DateTime(1986, 12, 30), Colors.Olive),
				new Person("Yvonne", new DateTime(1987, 1, 10), Colors.Purple),
				new Person("Zachary", new DateTime(1988, 2, 5), Colors.Red)
			};

			// Create the ListView.
			ListView listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				// Source of data items.
				ItemsSource = new List<List<Person>> { people, people2 },
				IsPullToRefreshEnabled = true,
				IsGroupingEnabled = true,

				// Define template for displaying each item.
				// (Argument of DataTemplate constructor is called for 
				//      each item; it must return a Cell derivative.)
				ItemTemplate = new MyDataTemplateSelector()
			};

			listView.Refreshing += async (sender, e) =>
			{
				await Task.Delay(5000);
				listView.IsRefreshing = false;
			};

			listView.ItemSelected += (sender, args) =>
			{
				if (listView.SelectedItem == null)
					return;
				listView.SelectedItem = null;
			};

			// Accomodate iPhone status bar.
			Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(10, 20, 10, 5) : new Thickness(10, 0, 10, 5);
			// Build the page.
			Content = new StackLayout
			{
				Children =
				{
					header,
					listView
				}
			};
		}
	}
}
