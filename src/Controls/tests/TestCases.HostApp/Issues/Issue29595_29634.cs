using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 29595, "iOS CV: GridItemsLayout not centering single item, Empty view not resizing when bounds change", PlatformAffected.iOS)]
	public class Issue29595_29634_Shell : Shell
	{
		public Issue29595_29634_Shell()
		{
			var tabbar = new TabBar
			{
				Items =
				{
					new Tab
					{
						Title = "Store 1",
						Items =
						{
							new ShellContent
							{
								ContentTemplate = new DataTemplate(() => new Issue29595_29634())
							}
						}
					},
					new Tab
					{
						Title = "Store 2",
						Items =
						{
							new ShellContent
							{
								ContentTemplate = new DataTemplate(() => new Issue29595_29634())
							}
						}
					},
					new Tab
					{
						Title = "Store 3",
						Items =
						{
							new ShellContent
							{
								ContentTemplate = new DataTemplate(() => new Issue29595_29634())
							}
						}
					},
				}
			};

			Items.Add(tabbar);
			
#if IOS
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
#endif
			FlyoutBehavior = FlyoutBehavior.Flyout;
			SetNavBarHasShadow(this, false);
			SetTabBarBackgroundColor(this, Colors.White);
			SetTabBarForegroundColor(this, Colors.Blue);
			SetTabBarTitleColor(this, Colors.Blue);
			SetTabBarUnselectedColor(this, Colors.Gray);
		}
	}

    public partial class Issue29595_29634 : ContentPage
    {
        public Issue29595_29634()
	    {
		    var issue29634 = new CollectionView
		    {
			    ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal) { ItemSpacing = 10 },
			    ItemTemplate = new DataTemplate(),
			    EmptyView = new Button
			    {
				    Margin=new Thickness(0,0,0,5),
				    BackgroundColor=Colors.LightSeaGreen,
				    FontSize = 12,
				    TextColor = Colors.DarkSlateGray,
				    Text="Button text"
			    }
		    };
		    Shell.SetTitleView(this, issue29634);
		    
		    var vm = new Issue29595ViewModel();
		    HideSoftInputOnTapped = true;
		    BindingContext = vm;

	        var grid = new Grid
	        {
	            Padding = 10
	        };

	        var issue29595 = new CollectionView
	        {
	            Margin = 10,
	            VerticalOptions = LayoutOptions.Fill,
	            ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)
	            {
	                HorizontalItemSpacing = 8,
	                VerticalItemSpacing = 8
	            },
	            ItemTemplate = GetItemTemplate()
	        };

	        issue29595.SetBinding(ItemsView.ItemsSourceProperty, "Items");
	        
	        grid.Add(issue29595);

	        Content = grid;
	    }

	    static DataTemplate GetItemTemplate(double fontSize = 14)
	    {
		    return new DataTemplate(() =>
		    {
			    var border = new Border
			    {
				    Stroke = new SolidColorBrush(Colors.Black),
				    StrokeShape = new RoundRectangle { CornerRadius = 15 }
			    };

			    var innerGrid = new Grid
			    {
				    BackgroundColor = Colors.Yellow,
				    RowDefinitions =
				    {
					    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
					    new RowDefinition { Height = GridLength.Auto }
				    }
			    };

			    var image = new FFImageLoadingStubImage
			    {
				    Aspect = Aspect.AspectFill,
				    Source = "dotnet_bot.png"
			    };
			    Grid.SetRow(image, 0);

			    var label = new Label
			    {
				    Text = "Test",
				    AutomationId = "StubLabel",
				    FontSize = fontSize,
				    FontFamily = "OpenSansRegular",
				    TextColor = Colors.Black,
				    HorizontalOptions = LayoutOptions.Center,
				    VerticalOptions = LayoutOptions.Center
			    };
			    Grid.SetRow(label, 1);

			    innerGrid.Add(image);
			    innerGrid.Add(label);

			    border.Content = innerGrid;
			    return border;
		    });
	    }

	    protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is Issue29595ViewModel viewModel)
            {
                await viewModel.Init();
            }
        }
    }

    public class FFImageLoadingStubImage : Image
    {
	    int counter = 0;
	    protected async override void OnPropertyChanged(string propertyName = null)
	    {
		    base.OnPropertyChanged(propertyName);

		    if (propertyName == SourceProperty.PropertyName)
		    {
			    ++counter;
			    await Task.Delay(100);
			    InvalidateMeasure();
		    }
	    }

	    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			var desiredSize = base.OnMeasure(double.PositiveInfinity, double.PositiveInfinity);
			var desiredWidth = double.IsNaN(desiredSize.Request.Width) ? 0 : desiredSize.Request.Width + counter;
			var desiredHeight = double.IsNaN(desiredSize.Request.Height) ? 0 : desiredSize.Request.Height;

			if (double.IsNaN(widthConstraint))
				widthConstraint = 0;
			if (double.IsNaN(heightConstraint))
				heightConstraint = 0;

			if (Math.Abs(desiredWidth) < double.Epsilon || Math.Abs(desiredHeight) < double.Epsilon)
				return new SizeRequest(new Size(0, 0));

			if (double.IsPositiveInfinity(widthConstraint) && double.IsPositiveInfinity(heightConstraint))
			{
				return new SizeRequest(new Size(desiredWidth, desiredHeight));
			}

			if (double.IsPositiveInfinity(widthConstraint))
			{
				var factor = heightConstraint / desiredHeight;
				return new SizeRequest(new Size(desiredWidth * factor, desiredHeight * factor));
			}

			if (double.IsPositiveInfinity(heightConstraint))
			{
				var factor = widthConstraint / desiredWidth;
				return new SizeRequest(new Size(desiredWidth * factor, desiredHeight * factor));
			}

			var fitsWidthRatio = widthConstraint / desiredWidth;
			var fitsHeightRatio = heightConstraint / desiredHeight;

			if (double.IsNaN(fitsWidthRatio))
				fitsWidthRatio = 0;
			if (double.IsNaN(fitsHeightRatio))
				fitsHeightRatio = 0;

			if (Math.Abs(fitsWidthRatio) < double.Epsilon && Math.Abs(fitsHeightRatio) < double.Epsilon)
				return new SizeRequest(new Size(0, 0));

			if (Math.Abs(fitsWidthRatio) < double.Epsilon)
				return new SizeRequest(new Size(desiredWidth * fitsHeightRatio, desiredHeight * fitsHeightRatio));

			if (Math.Abs(fitsHeightRatio) < double.Epsilon)
				return new SizeRequest(new Size(desiredWidth * fitsWidthRatio, desiredHeight * fitsWidthRatio));

			var ratioFactor = Math.Min(fitsWidthRatio, fitsHeightRatio);

			return new SizeRequest(new Size(desiredWidth * ratioFactor, desiredHeight * ratioFactor));
		}
    }

    public class Issue29595ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ToggleCommand { get; }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Issue29595ViewModel()
        {
            ToggleCommand = new Command(async () =>
            {
	            if (Items.Count == 1)
	            {
		            await LoadManyItems();
	            }
	            else
	            {
		            await LoadOneItem();
	            }
            });
        }

        public async Task LoadManyItems()
        {
            IsBusy = true;

            Items = [];

            await Task.Delay(200);

            for (int i = 0; i < 5; i++)
            {
                var item = new PlatformOption
                {
                    Name = $"item {i}"
                };

                Items.Add(item);
            }

            IsBusy = false;
        }

        public async Task LoadOneItem()
        {
            await LoadInitialItems();
        }

        public async Task Init()
        {
            await LoadInitialItems();
        }

        private async Task LoadInitialItems()
        {
            IsBusy = true;

            Items = [];

            await Task.Delay(200);

            var item = new PlatformOption
            {
                Name = "item1"
            };
            Items.Add(item);

            IsBusy = false;
        }
        
        

        private PlatformOptionCollection _items = [];

        public PlatformOptionCollection Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public class PlatformOption
    {
        public string Name { get; set; }
    }

    public class PlatformOptionCollection : ObservableCollection<PlatformOption>
    {
        public PlatformOptionCollection()
        {

        }

        public PlatformOptionCollection(IEnumerable<PlatformOption> collection) : base(collection)
        {

        }
    }
}
