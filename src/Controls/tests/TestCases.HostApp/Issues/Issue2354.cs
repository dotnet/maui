namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2354, "ListView, ImageCell and disabled source cache and same image url", PlatformAffected.iOS | PlatformAffected.Android, isInternetRequired: true)]
	public class Issue2354 : TestContentPage
	{
		protected override void Init()
		{
			var presidents = new List<President>();

			presidents.Add(new President($"Presidente 44", 1, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/avatar.png"));
			presidents.Add(new President($"Presidente 43", 2, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/oasis.jpg"));
			presidents.Add(new President($"Presidente 42", 3, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/photo21314.jpg"));
			presidents.Add(new President($"Presidente 41", 4, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/groceries.png"));
			presidents.Add(new President($"Presidente 40", 5, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/avatar.png"));
			presidents.Add(new President($"Presidente 39", 6, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/oasis.jpg"));
			presidents.Add(new President($"Presidente 38", 7, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/photo21314.jpg"));
			presidents.Add(new President($"Presidente 37", 8, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/groceries.png"));
			presidents.Add(new President($"Presidente 36", 9, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/avatar.png"));
			presidents.Add(new President($"Presidente 35", 10, $"https://raw.githubusercontent.com/dotnet/maui/main/src/Controls/tests/TestCases.HostApp/Resources/Images/oasis.jpg"));

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