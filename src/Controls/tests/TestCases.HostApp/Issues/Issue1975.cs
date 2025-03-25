using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1975, "[iOS] ListView throws NRE when grouping enabled and data changed",
		PlatformAffected.iOS)]
	public class Issue1975 : NavigationPage
	{
		public Issue1975() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(CreateRootPage());
			}

			const string Success = "If you can see this, the test has passed";
			const string Go = "Go";

			ContentPage CreateRootPage()
			{
				var button = new Button { AutomationId = Go, Text = Go };

				button.Clicked += (sender, args) => Application.Current.MainPage = ModifyDataPage();

				var lv = new ListView();

				lv.SetBinding(ListView.ItemsSourceProperty, new Binding("Items"));
				lv.IsGroupingEnabled = true;
				lv.GroupDisplayBinding = new Binding("Description");
#if !WINDOWS
				//It appears that the ListView is not detectable in the CI environment
				//For more information : https://github.com/dotnet/maui/issues/27336
				lv.GroupShortNameBinding = new Binding("ShortName");
#endif
				lv.ItemTemplate = new DataTemplate(() =>
				{
					var textCell = new TextCell();
					textCell.SetBinding(TextCell.TextProperty, new Binding("Text"));
					return textCell;
				});

				var layout = new StackLayout();
				layout.Children.Add(button);
				layout.Children.Add(lv);

				return new ContentPage { Content = layout, BindingContext = DataSample.Instance };
			}

			ContentPage ModifyDataPage()
			{
				var contentPage = new ContentPage { Content = new Label { AutomationId = Success, Text = Success, Margin = 100 } };

				contentPage.Appearing += (sender, args) =>
					DataSample.Instance.Items.Add(new Item("C") { new SubItem("Cherry"), new SubItem("Cranberry") });

				return contentPage;
			}


			class DataSample
			{
				static readonly object _lockObject = new object();

				static volatile DataSample _instance;

				public static DataSample Instance
				{
					get
					{
						if (_instance != null)
						{
							return _instance;
						}

						lock (_lockObject)
						{
							if (_instance == null)
							{
								_instance = new DataSample();
							}
						}

						return _instance;
					}
				}

				DataSample()
				{
					Items = new ObservableCollection<Item>
				{
					new Item("A")
					{
						new SubItem("Apple"),
						new SubItem("Avocado")
					},
					new Item("B")
					{
						new SubItem("Banana"),
						new SubItem("Blackberry")
					}
				};
				}

				public ObservableCollection<Item> Items { get; }
			}


			class Item : ObservableCollection<SubItem>
			{
				public string ShortName { get; set; }
				public string Description { get; set; }

				public Item(string shortName)
				{
					ShortName = shortName;
					Description = shortName;
				}
			}


			class SubItem
			{
				public string Text { get; set; }

				public SubItem(string text)
				{
					Text = text;
				}
			}
		}
	}
}