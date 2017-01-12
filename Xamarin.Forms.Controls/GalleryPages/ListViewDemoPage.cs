using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls
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

		    public MyDataTemplateSelector ()
		    {
			    _evenTemplate = new DataTemplate (() => {
				    // Create views with bindings for displaying each property.
				    Label nameLabel = new Label ();
				    nameLabel.SetBinding (Label.TextProperty, "Name");

				    Label birthdayLabel = new Label ();
				    birthdayLabel.SetBinding (Label.TextProperty,
											  new Binding ("Birthday", BindingMode.OneWay,
														   null, null, "Born {0:d}"));

				    BoxView boxView = new BoxView ();
				    boxView.SetBinding (BoxView.ColorProperty, "FavoriteColor");

				    // Return an assembled ViewCell.
				    return new ViewCell {
					    View = new StackLayout {
						    Padding = new Thickness (0, 5),
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
			    });

			    _oddTemplate = new DataTemplate (() => {
				    // Create views with bindings for displaying each property.
				    Label nameLabel = new Label ();
				    nameLabel.SetBinding (Label.TextProperty, "Name");

				    Label birthdayLabel = new Label ();
				    birthdayLabel.SetBinding (Label.TextProperty,
											  new Binding ("Birthday", BindingMode.OneWay,
														   null, null, "Born {0:d}"));

				    BoxView boxView = new BoxView ();
				    boxView.SetBinding (BoxView.ColorProperty, "FavoriteColor");

				    // Return an assembled ViewCell.
				    return new ViewCell {
					    View = new StackLayout {
						    Padding = new Thickness (0, 5),
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
			    });
		    }

		    protected override DataTemplate OnSelectTemplate (object item, BindableObject container)
		    {
			    return ((Person)item).Birthday.Month % 2 == 0 ? _evenTemplate : _oddTemplate;
		    }
	    }

        public ListViewDemoPage()
        {
            Label header = new Label
            {
                Text = "ListView",
#pragma warning disable 618
                Font = Font.BoldSystemFontOfSize(50),
#pragma warning restore 618
                HorizontalOptions = LayoutOptions.Center
            };

            // Define some data.
            List<Person> people = new List<Person>
            {
                new Person("Abigail", new DateTime(1975, 1, 15), Color.Aqua),
                new Person("Bob", new DateTime(1976, 2, 20), Color.Black),
                new Person("Cathy", new DateTime(1977, 3, 10), Color.Blue),
#pragma warning disable 618
                new Person("David", new DateTime(1978, 4, 25), Color.Fuschia),
#pragma warning restore 618
                new Person("Eugenie", new DateTime(1979, 5, 5), Color.Gray),
                new Person("Freddie", new DateTime(1980, 6, 30), Color.Green),
                new Person("Greta", new DateTime(1981, 7, 15), Color.Lime),
                new Person("Harold", new DateTime(1982, 8, 10), Color.Maroon),
                new Person("Irene", new DateTime(1983, 9, 25), Color.Navy),
                new Person("Jonathan", new DateTime(1984, 10, 10), Color.Olive),
                new Person("Kathy", new DateTime(1985, 11, 20), Color.Purple),
                new Person("Larry", new DateTime(1986, 12, 5), Color.Red),
                new Person("Monica", new DateTime(1975, 1, 5), Color.Silver),
                new Person("Nick", new DateTime(1976, 2, 10), Color.Teal),
                new Person("Olive", new DateTime(1977, 3, 20), Color.White),
                new Person("Pendleton", new DateTime(1978, 4, 10), Color.Yellow),
                new Person("Queenie", new DateTime(1979, 5, 15), Color.Aqua),
                new Person("Rob", new DateTime(1980, 6, 30), Color.Blue),
#pragma warning disable 618
                new Person("Sally", new DateTime(1981, 7, 5), Color.Fuschia),
#pragma warning restore 618
                new Person("Timothy", new DateTime(1982, 8, 30), Color.Green),
                new Person("Uma", new DateTime(1983, 9, 10), Color.Lime),
                new Person("Victor", new DateTime(1984, 10, 20), Color.Maroon),
                new Person("Wendy", new DateTime(1985, 11, 5), Color.Navy),
                new Person("Xavier", new DateTime(1986, 12, 30), Color.Olive),
                new Person("Yvonne", new DateTime(1987, 1, 10), Color.Purple),
                new Person("Zachary", new DateTime(1988, 2, 5), Color.Red)
            };
			List<Person> people2 = new List<Person>
            {
                new Person("Abigail", new DateTime(1975, 1, 15), Color.Aqua),
                new Person("Bob", new DateTime(1976, 2, 20), Color.Black),
                new Person("Cathy", new DateTime(1977, 3, 10), Color.Blue),
#pragma warning disable 618
                new Person("David", new DateTime(1978, 4, 25), Color.Fuschia),
#pragma warning restore 618
                new Person("Eugenie", new DateTime(1979, 5, 5), Color.Gray),
                new Person("Freddie", new DateTime(1980, 6, 30), Color.Green),
                new Person("Greta", new DateTime(1981, 7, 15), Color.Lime),
                new Person("Harold", new DateTime(1982, 8, 10), Color.Maroon),
                new Person("Irene", new DateTime(1983, 9, 25), Color.Navy),
                new Person("Jonathan", new DateTime(1984, 10, 10), Color.Olive),
                new Person("Kathy", new DateTime(1985, 11, 20), Color.Purple),
                new Person("Larry", new DateTime(1986, 12, 5), Color.Red),
                new Person("Monica", new DateTime(1975, 1, 5), Color.Silver),
                new Person("Nick", new DateTime(1976, 2, 10), Color.Teal),
                new Person("Olive", new DateTime(1977, 3, 20), Color.White),
                new Person("Pendleton", new DateTime(1978, 4, 10), Color.Yellow),
                new Person("Queenie", new DateTime(1979, 5, 15), Color.Aqua),
                new Person("Rob", new DateTime(1980, 6, 30), Color.Blue),
#pragma warning disable 618
                new Person("Sally", new DateTime(1981, 7, 5), Color.Fuschia),
#pragma warning restore 618
                new Person("Timothy", new DateTime(1982, 8, 30), Color.Green),
                new Person("Uma", new DateTime(1983, 9, 10), Color.Lime),
                new Person("Victor", new DateTime(1984, 10, 20), Color.Maroon),
                new Person("Wendy", new DateTime(1985, 11, 5), Color.Navy),
                new Person("Xavier", new DateTime(1986, 12, 30), Color.Olive),
                new Person("Yvonne", new DateTime(1987, 1, 10), Color.Purple),
                new Person("Zachary", new DateTime(1988, 2, 5), Color.Red)
            };

            // Create the ListView.
            ListView listView = new ListView (ListViewCachingStrategy.RecycleElement)
            {
                // Source of data items.
                ItemsSource = new List<List<Person>> {people, people2},
				IsPullToRefreshEnabled = true,
				IsGroupingEnabled = true,

                // Define template for displaying each item.
                // (Argument of DataTemplate constructor is called for 
                //      each item; it must return a Cell derivative.)
                ItemTemplate = new MyDataTemplateSelector ()
            };

	        listView.Refreshing += async (sender, e) => {
		        await Task.Delay (5000);
		        listView.IsRefreshing = false;
	        };

	        listView.ItemSelected += (sender, args) => {
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
