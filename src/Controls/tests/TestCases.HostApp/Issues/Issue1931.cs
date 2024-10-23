using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1931,
		"Xamarin Forms on Android: ScrollView on ListView header crashes app when closing page",
		PlatformAffected.Android)]
	public class Issue1931 : NavigationPage
	{
		public Issue1931() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			const string Go = "Go";
			const string Back = "GoBack";
			const string Success = "Success";

			Label _result;
			Label _instructions2;

			public MainPage()
			{
				Navigation.PushAsync(CreateRootPage());
			}

			ContentPage CreateRootPage()
			{
				var page = new ContentPage();
				page.Title = "GH1931 Root";

				var button = new Button { Text = Go, AutomationId = Go };
				button.Clicked += (sender, args) => Navigation.PushAsync(ListViewPage());

				var buttonBack = new Button { Text = "back" };
				buttonBack.Clicked += (sender, args) => Navigation.PopModalAsync();

				var instructions = new Label { Text = $"Tap the {Go} button" };

				_result = new Label { Text = Success, IsVisible = false };
				_instructions2 = new Label { Text = "If you can see this, the test has passed", IsVisible = false };

				var layout = new StackLayout();
				layout.Children.Add(instructions);
				layout.Children.Add(button);
				layout.Children.Add(_result);
				layout.Children.Add(_instructions2);
				layout.Children.Add(buttonBack);
				page.Content = layout;

				return page;
			}


			public class Item2 : System.ComponentModel.INotifyPropertyChanged
			{
				public string Name
				{
					get => name;
					set
					{
						name = value;
						OnPropertyChanged("Name");
					}
				}
				private string name;
				public int TapCount
				{
					get => tapCount;
					set
					{
						tapCount = value;
						OnPropertyChanged("TapCount");
						IsEnabled = !IsEnabled;
					}
				}
				private int tapCount;

				public bool IsEnabled
				{
					get => isEnabled;
					set
					{
						isEnabled = value;
						OnPropertyChanged("IsEnabled");
					}
				}
				private bool isEnabled;

				public Item2()
				{
				}

				public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
				protected virtual void OnPropertyChanged(string propertyName)
				{
					PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
				}
			}

			public ObservableCollection<Item2> Source = new ObservableCollection<Item2>()
				{
					new Item2() { Name = "Winner", TapCount = 0 },
					new Item2() { Name = "Winner", TapCount = 0 },
					new Item2() { Name = "Chicken", TapCount = 0 },
					new Item2() { Name = "Dinner", TapCount = 0 }
		};

			ContentPage ListViewPage()
			{
				var page = new ContentPage();

				var layout = new StackLayout();

				var listView = new ListView();

				var scrollView = new ScrollView { Content = new BoxView { Color = Colors.Green } };

				listView.Header = scrollView;
				listView.RowHeight = 40;
				listView.SetBinding(ListView.ItemsSourceProperty, ".");
				listView.BindingContext = Source;
				listView.ItemTemplate = new DataTemplate(() =>
				{
					var viewCell = new ViewCell();
					var label = new Label();
					label.SetBinding(Label.TextProperty, "Name");
					viewCell.View = label;
					viewCell.SetBinding(ViewCell.IsEnabledProperty, "IsEnabled");
					return viewCell;
				});
				page.Title = "GH1931 Test";

				listView.ItemTapped += async (object sender, Microsoft.Maui.Controls.ItemTappedEventArgs e) =>
			   {
				   (e.Item as Item2).TapCount++;
				   await Navigation.PopAsync();
			   };

				var instructions = new Label { Text = $"Tap the {Back} button" };

				var button = new Button { Text = Back, AutomationId = Back };
				button.Clicked += (sender, args) =>
				{
					Navigation.PopAsync();
					Source[2].TapCount++;
				};

				layout.Children.Add(instructions);
				layout.Children.Add(button);
				layout.Children.Add(listView);

				page.Content = layout;

				page.Appearing += (sender, args) =>
				{
					_instructions2.IsVisible = true;
					_result.IsVisible = true;
				};

				return page;
			}
		}
	}
}