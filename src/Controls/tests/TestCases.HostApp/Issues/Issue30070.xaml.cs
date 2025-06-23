using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 30070,
		"ScrollView Orientation set to Horizontal allows both horizontal and vertical scrolling",
		PlatformAffected.iOS)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue30070 : TestContentPage
	{
		Issue30070ViewModel _viewModel;

		public Issue30070()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			_viewModel = new Issue30070ViewModel();
			BindingContext = _viewModel;
		}

		void OnContentTypeButtonClicked(object sender, EventArgs e)
		{
			if (sender is not Button btn)
			{
				return;
			}

			if (BindingContext is not Issue30070ViewModel vm)
			{
				return;
			}

			switch (btn.Text)
			{
				case "Label":
					vm.Content = new Label
					{
						Text = string.Join(Environment.NewLine,
							Enumerable.Range(1, 100).Select(i =>
								$"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim. {i}")),
						FontSize = 16,
						Padding = 10
					};
					break;
				case "Image":
					vm.Content = new Image
					{
						Source = "dotnet_bot.png",
						HeightRequest = 2000,
						WidthRequest = 2000,
						Aspect = Aspect.AspectFit
					};
					break;
				case "Editor":
					vm.Content = new Editor
					{
						Text = string.Join(Environment.NewLine,
							Enumerable.Range(1, 100).Select(i =>
								$"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim. {i}")),
						AutoSize = EditorAutoSizeOption.TextChanges
					};
					break;
				case "Grid":
					var grid = new Grid();
					for (int row = 0; row < 30; row++)
					{
						grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
					}

					for (int col = 0; col < 20; col++)
					{
						grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
					}

					string[] names =
					[
						"Apple", "Banana", "Tomato", "Potato", "Orange", "Cucumber", "Broccoli", "Pineapple",
						"Strawberry", "Onion", "Lettuce", "Pear", "Kiwi", "Radish", "Cabbage", "Melon", "Plum",
						"Garlic", "Corn", "Blueberry", "Zucchini", "Bell Pepper", "Beetroot", "Avocado",
						"Asparagus", "Pomegranate", "Cauliflower", "Fennel", "Chili Pepper", "Mushroom", "Turnip",
						"Tangerine", "Radicchio", "Passion Fruit", "Endive", "Starfruit", "Kale", "Guava", "Chard",
						"Persimmon", "Arugula", "Coconut", "Celery", "Lychee", "Okra", "Dragon Fruit", "Squash",
						"Mulberry", "Artichoke", "Cranberry", "Parsnip", "Raspberry", "Pumpkin", "Blackberry",
						"Sweet Potato", "Grapefruit", "Eggplant", "Tamarind", "Nectarine"
					];

					for (int row = 0; row < 30; row++)
					for (int col = 0; col < 20; col++)
					{
						int index = (row * 20 + col) % names.Length;
						var cell = new Label
						{
							Text = names[index],
							FontSize = 16,
							HorizontalOptions = LayoutOptions.Center,
							Padding = new Thickness(8)
						};
						grid.Add(cell, col, row);
					}

					vm.Content = grid;
					break;

				case "AbsoluteLayout":
					var absolute = new AbsoluteLayout { HeightRequest = 800, WidthRequest = 800 };

					for (int i = 0; i < 10; i++)
					{
						var box = new BoxView
						{
							Color = i % 2 == 0 ? Colors.CornflowerBlue : Colors.Orange,
							WidthRequest = 80,
							HeightRequest = 80
						};
						AbsoluteLayout.SetLayoutBounds(box, new Rect(30 + i * 70, 30 + i * 70, 80, 80));
						absolute.Children.Add(box);
					}

					for (int i = 0; i < 10; i++)
					{
						var box = new BoxView
						{
							Color = i % 2 == 0 ? Colors.MediumPurple : Colors.ForestGreen,
							WidthRequest = 80,
							HeightRequest = 80
						};
						AbsoluteLayout.SetLayoutBounds(box, new Rect(30 + (9 - i) * 70, 30 + i * 70, 80, 80));
						absolute.Children.Add(box);
					}

					vm.Content = absolute;
					break;
			}
		}
	}

	public class Issue30070ViewModel : INotifyPropertyChanged
	{
		string _contentText;
		View _content;
	 
		public Issue30070ViewModel()
		{
			Content = new Label
			{
				Text = string.Join(Environment.NewLine, Enumerable.Range(1, 100).Select(i => $"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim, eget facilisis enim nisl nec elit . Sed euismod, urna eu tincidunt consectetur, nisi nisl aliquam enim Eget facilisis enim nisl nec elit Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae. Nullam ac erat at dui laoreet aliquet. Praesent euismod, justo at dictum facilisis, urna erat dictum enim. {i}")),
				FontSize = 18,
				Padding = 10
			};
		}
	 
		public string ContentText
		{
			get => _contentText;
			set
			{
				if (_contentText != value)
				{
					_contentText = value;
					Content = new Label { Text = _contentText }; // Update Content when ContentText changes
					OnPropertyChanged();
				}
			}
		}


		public View Content
		{
			get => _content;
			set
			{
				if (_content != value)
				{
					_content = value;
					OnPropertyChanged();
				}
			}
		}
		
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}