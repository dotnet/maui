namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2354, "ListView, ImageCell and disabled source cache and same image url", PlatformAffected.iOS | PlatformAffected.Android, isInternetRequired: true)]
	public class Issue2354 : TestContentPage
	{
		protected override void Init()
		{
			var presidents = new List<President>();

			presidents.Add(new President($"Presidente 44", 1, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/avatar.png?raw=true"));
			presidents.Add(new President($"Presidente 43", 2, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/oasis.jpg?raw=true"));
			presidents.Add(new President($"Presidente 42", 3, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/photo21314.jpg?raw=true"));
			presidents.Add(new President($"Presidente 41", 4, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/groceries.png?raw=true"));
			presidents.Add(new President($"Presidente 40", 5, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/avatar.png?raw=true"));
			presidents.Add(new President($"Presidente 39", 6, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/oasis.jpg?raw=true"));
			presidents.Add(new President($"Presidente 38", 7, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/photo21314.jpg?raw=true"));
			presidents.Add(new President($"Presidente 37", 8, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/groceries.png?raw=true"));
			presidents.Add(new President($"Presidente 36", 9, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/avatar.png?raw=true"));
			presidents.Add(new President($"Presidente 35", 10, $"https://github.com/dotnet/maui/blob/main/src/Controls/tests/TestCases.HostApp/Resources/Images/oasis.jpg?raw=true"));

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