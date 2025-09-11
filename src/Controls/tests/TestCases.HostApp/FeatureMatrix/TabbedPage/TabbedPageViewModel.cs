using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public class TabbedPageViewModel : INotifyPropertyChanged
{
	private Brush _barBackground = new SolidColorBrush(Colors.Transparent);
	private Color _barTextColor = Colors.Red;
	private Color _selectedTabColor = Colors.Orange;
	private Color _unselectedTabColor = Colors.LightGray;
	private bool _isEnabled = true;
	private FlowDirection _flowDirection = FlowDirection.LeftToRight;
	private ObservableCollection<TabbedPageItemSource> _itemsSource;
	private DataTemplate _itemTemplate;
	private object _selectedItem;
#if ANDROID
	public ObservableCollection<TabbedPageItemSource> ItemsSourceOne { get; } = new ObservableCollection<TabbedPageItemSource>
	{
		new TabbedPageItemSource { Name = "TAB 1", Id = "Tab1Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 2", Id = "Tab2Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 3", Id = "Tab3Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 4", Id = "Tab4Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 5", Id = "Tab5Label", ImageUrl = "dotnet_bot.png" }
	};
	public ObservableCollection<TabbedPageItemSource> ItemsSourceTwo { get; } = new ObservableCollection<TabbedPageItemSource>
	{
		new TabbedPageItemSource { Name = "APPLE", Id="AppleLabel", ImageUrl = "apple.png" },
		new TabbedPageItemSource { Name = "CHERRY", Id="CherryLabel", ImageUrl = "cherry.png" },
		new TabbedPageItemSource { Name = "GRAPE", Id="GrapeLabel", ImageUrl = "grape.png" },
		new TabbedPageItemSource { Name = "KIWI", Id="KiwiLabel", ImageUrl = "kiwi.png" },
		new TabbedPageItemSource { Name = "MANGO", Id="MangoLabel", ImageUrl = "mango.png" }
	};
#else
	public ObservableCollection<TabbedPageItemSource> ItemsSourceOne { get; } = new ObservableCollection<TabbedPageItemSource>
	{
		new TabbedPageItemSource { Name = "TAB 1", Id="Tab1Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 2", Id="Tab2Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 3", Id="Tab3Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 4", Id="Tab4Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 5", Id="Tab5Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 6", Id="Tab6Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 7", Id="Tab7Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 8", Id="Tab8Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 9", Id="Tab9Label", ImageUrl = "dotnet_bot.png" },
		new TabbedPageItemSource { Name = "TAB 10", Id="Tab10Label", ImageUrl = "dotnet_bot.png" }
	};
	public ObservableCollection<TabbedPageItemSource> ItemsSourceTwo { get; } = new ObservableCollection<TabbedPageItemSource>
	{
		new TabbedPageItemSource { Name = "APPLE", Id="AppleLabel", ImageUrl = "apple.png" },
		new TabbedPageItemSource { Name = "CHERRY", Id="CherryLabel", ImageUrl = "cherry.png" },
		new TabbedPageItemSource { Name = "GRAPE", Id="GrapeLabel", ImageUrl = "grape.png" },
		new TabbedPageItemSource { Name = "KIWI", Id="KiwiLabel", ImageUrl = "kiwi.png" },
		new TabbedPageItemSource { Name = "LEMON", Id="LemonLabel", ImageUrl = "lemon.png" },
		new TabbedPageItemSource { Name = "MANGO", Id="MangoLabel", ImageUrl = "mango.png" },
		new TabbedPageItemSource { Name = "ORANGE", Id="OrangeLabel", ImageUrl = "orange.png" },
		new TabbedPageItemSource { Name = "PINEAPPLE", Id="PineappleLabel", ImageUrl = "pineapple.png" },
		new TabbedPageItemSource { Name = "POMEGRANATE", Id="PomegranateLabel", ImageUrl = "pomegranate.png" },
		new TabbedPageItemSource { Name = "STRAWBERRY", Id="StrawberryLabel", ImageUrl = "strawberry.png" }
	};
#endif
	public event PropertyChangedEventHandler PropertyChanged;

	public TabbedPageViewModel()
	{
		ItemsSource = ItemsSourceOne;
		SelectedItem = ItemsSourceOne.FirstOrDefault();
		ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 18,
				HorizontalOptions = LayoutOptions.Center
			};
			label.SetBinding(Label.AutomationIdProperty, "Id");
			label.SetBinding(Label.TextProperty, "Name");

			var image = new Image
			{
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 200,
				HeightRequest = 200
			};
			image.SetBinding(Image.SourceProperty, "ImageUrl");

			var page = new ContentPage
			{
				AutomationId = "ContentPageOne",
				IconImageSource = "bank.png",
				Content = new StackLayout
				{
					Padding = new Thickness(5, 25),
					Children =
					{
						new Label
						{
							Text = "Template One",
							FontAttributes = FontAttributes.Bold,
							FontSize = 18,
							HorizontalOptions = LayoutOptions.Center
						},
						label,
						image
					}
				}
			};
			page.SetBinding(ContentPage.TitleProperty, new Binding("Name"));
			return page;
		});
	}

	public ObservableCollection<TabbedPageItemSource> ItemsSource
	{
		get => _itemsSource;
		set
		{
			if (_itemsSource != value)
			{
				_itemsSource = value;
				OnPropertyChanged();
			}
		}
	}

	public DataTemplate ItemTemplate
	{
		get => _itemTemplate;
		set
		{
			if (_itemTemplate != value)
			{
				_itemTemplate = value;
				OnPropertyChanged();
			}
		}
	}

	public object SelectedItem
	{
		get => _selectedItem;
		set
		{
			if (_selectedItem != value)
			{
				_selectedItem = value;
				OnPropertyChanged();
			}
		}
	}

	public Brush BarBackground
	{
		get => _barBackground;
		set
		{
			if (_barBackground != value)
			{
				_barBackground = value;
				OnPropertyChanged();
			}
		}
	}

	public Color BarTextColor
	{
		get => _barTextColor;
		set
		{
			if (_barTextColor != value)
			{
				_barTextColor = value;
				OnPropertyChanged();
			}
		}
	}

	public Color SelectedTabColor
	{
		get => _selectedTabColor;
		set
		{
			if (_selectedTabColor != value)
			{
				_selectedTabColor = value;
				OnPropertyChanged();
			}
		}
	}

	public Color UnselectedTabColor
	{
		get => _unselectedTabColor;
		set
		{
			if (_unselectedTabColor != value)
			{
				_unselectedTabColor = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public FlowDirection FlowDirection
	{
		get => _flowDirection;
		set
		{
			if (_flowDirection != value)
			{
				_flowDirection = value;
				OnPropertyChanged();
			}
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class TabbedPageItemSource
{
	public string Name { get; set; }
	public string Id { get; set; }
	public string ImageUrl { get; set; }
}