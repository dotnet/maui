using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;

namespace Maui.Controls.Sample
{
    public enum CarouselItemsSourceType
    {
        None,
        ObservableCollectionT,
    }

    public class CarouselViewViewModel : INotifyPropertyChanged
    {
        private object _emptyView;
        private DataTemplate _emptyViewTemplate;
        private DataTemplate _itemTemplate;
        private CarouselItemsSourceType _itemsSourceType = CarouselItemsSourceType.ObservableCollectionT;
        private bool _isLoopEnabled = false;
        private bool _isSwipeEnabled = true;
        private Thickness _peekAreaInsets;
        private int _position;
        private bool _isIndicatorViewVisible = true;
        private IItemsLayout _itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
        private ScrollBarVisibility _horizontalScrollBarVisibility = ScrollBarVisibility.Default;
        private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Default;
        private ItemsUpdatingScrollMode _itemsUpdatingScrollMode;
        private double _leftPeekInset;
        private double _rightPeekInset;
        private string _previousItemText;
        private string _currentItemText;
        private string _currentItemPosition;
        private string _previousItemPosition;

        public CarouselViewViewModel()
        {
            LoadItems();
            AddItemCommand = new Command(AddItem);
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    Margin = new Thickness(10),
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };
                label.SetBinding(Label.TextProperty, ".");
                return label;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand AddItemCommand { get; }

        public ObservableCollection<string> Items { get; private set; }

        public object EmptyView
        {
            get => _emptyView;
            set { _emptyView = value; OnPropertyChanged(); }
        }

        public DataTemplate EmptyViewTemplate
        {
            get => _emptyViewTemplate;
            set { _emptyViewTemplate = value; OnPropertyChanged(); }
        }

        public DataTemplate ItemTemplate
        {
            get => _itemTemplate;
            set { _itemTemplate = value; OnPropertyChanged(); }
        }

        public CarouselItemsSourceType ItemsSourceType
        {
            get => _itemsSourceType;
            set
            {
                if (_itemsSourceType != value)
                {
                    _itemsSourceType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ItemsSource));
                }
            }
        }

        public bool Loop
        {
            get => _isLoopEnabled;
            set
            {
                if (_isLoopEnabled != value)
                {
                    _isLoopEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSwipeEnabled
        {
            get => _isSwipeEnabled;
            set
            {
                if (_isSwipeEnabled != value)
                {
                    _isSwipeEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsIndicatorViewVisible
        {
            get => _isIndicatorViewVisible;
            set
            {
                if (_isIndicatorViewVisible != value)
                {
                    _isIndicatorViewVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public Thickness PeekAreaInsets
        {
            get => _peekAreaInsets;
            set
            {
                if (_peekAreaInsets != value)
                {
                    _peekAreaInsets = value;
                    OnPropertyChanged();
                }
            }
        }

        public double LeftPeekInset
        {
            get => _leftPeekInset;
            set
            {
                if (_leftPeekInset != value)
                {
                    _leftPeekInset = value;
                    PeekAreaInsets = new Thickness(_leftPeekInset, 0, _rightPeekInset, 0);
                    OnPropertyChanged();
                }
            }
        }

        public double RightPeekInset
        {
            get => _rightPeekInset;
            set
            {
                if (_rightPeekInset != value)
                {
                    _rightPeekInset = value;
                    PeekAreaInsets = new Thickness(_leftPeekInset, 0, _rightPeekInset, 0);
                    OnPropertyChanged();
                }
            }
        }

        public int Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PreviousItemText
        {
            get => _previousItemText;
            set { _previousItemText = value; OnPropertyChanged(); }
        }

        public string CurrentItemText
        {
            get => _currentItemText;
            set { _currentItemText = value; OnPropertyChanged(); }
        }

        public string CurrentItemPostion
        {
            get => _currentItemPosition;
            set
            {
                if (_currentItemPosition != value)
                {
                    _currentItemPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PreviousItemPosition
        {
            get => _previousItemPosition;
            set
            {
                if (_previousItemPosition != value)
                {
                    _previousItemPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => _horizontalScrollBarVisibility;
            set
            {
                if (_horizontalScrollBarVisibility != value)
                {
                    _horizontalScrollBarVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => _verticalScrollBarVisibility;
            set
            {
                if (_verticalScrollBarVisibility != value)
                {
                    _verticalScrollBarVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public ItemsUpdatingScrollMode ItemsUpdatingScrollMode
        {
            get => _itemsUpdatingScrollMode;
            set
            {
                if (_itemsUpdatingScrollMode != value)
                {
                    _itemsUpdatingScrollMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public IItemsLayout ItemsLayout
        {
            get => _itemsLayout;
            set
            {
                if (_itemsLayout != value)
                {
                    _itemsLayout = value;
                    OnPropertyChanged();
                }
            }
        }

        public object ItemsSource
        {
            get
            {
                return ItemsSourceType switch
                {
                    CarouselItemsSourceType.ObservableCollectionT => Items,
                    CarouselItemsSourceType.None => null,
                    _ => Items
                };
            }
        }

        private void LoadItems()
        {
            Items = new ObservableCollection<string>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5"
            };
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(ItemsSource));
        }

        private void AddItem()
        {
            Items.Insert(0, $"Item {Items.Count + 1}");
            OnPropertyChanged(nameof(Items));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}