using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;

namespace Maui.Controls.Sample
{
    public enum CarouselViewItemsSourceType
    {
        None,
        ObservableCollection,
    }

    public class CarouselViewViewModel : INotifyPropertyChanged
    {
        private object _emptyView;
        private DataTemplate _emptyViewTemplate;
        private DataTemplate _itemTemplate;
        private CarouselViewItemsSourceType _itemsSourceType = CarouselViewItemsSourceType.ObservableCollection;
        private bool _isLoopEnabled = true;
        private bool _isSwipeEnabled = true;
        private bool _isBounceEnabled = true;
        private bool _isScrollAnimated = true;
        private Thickness _peekAreaInsets;
        private int _position;
        private object _currentItem;
        private IItemsLayout _itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

        private ScrollBarVisibility _horizontalScrollBarVisibility = ScrollBarVisibility.Default;
        private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Default;
        private ItemsUpdatingScrollMode _itemsUpdatingScrollMode;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }

        public CarouselViewViewModel()
        {
            LoadItems();
            AddItemCommand = new Command(AddItem);
            RemoveItemCommand = new Command(RemoveItem);
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

        public ObservableCollection<string> Items { get; private set; }

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
            // Add a new item to the Items collection
            Items.Insert(0, $"Item {Items.Count + 1}");
            OnPropertyChanged(nameof(Items));
        }
        private void RemoveItem()
        {
            // Remove the top item from the Items collection
            if (Items.Count > 0)
            {
                Items.RemoveAt(0);
                OnPropertyChanged(nameof(Items));
            }
        }

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

        public CarouselViewItemsSourceType ItemsSourceType
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
                    OnPropertyChanged(nameof(ItemsSource));

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

        public bool IsBounceEnabled
        {
            get => _isBounceEnabled;
            set
            {
                if (_isBounceEnabled != value)
                {
                    _isBounceEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsScrollAnimated
        {
            get => _isScrollAnimated;
            set
            {
                if (_isScrollAnimated != value)
                {
                    _isScrollAnimated = value;
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

        private double _leftPeekInset;
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

        private double _rightPeekInset;
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

        public object CurrentItem
        {
            get => _currentItem;
            set
            {
                if (!object.Equals(_currentItem, value))
                {
                    _currentItem = value;
                    if (_currentItem != null && Items != null)
                    {
                        int index = Items.IndexOf(_currentItem.ToString());
                        if (index != -1 && index != _position)
                        {
                            _position = index;
                            OnPropertyChanged(nameof(Position));
                        }
                    }
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
                    CarouselViewItemsSourceType.ObservableCollection => Items,
                    CarouselViewItemsSourceType.None => null,
                    _ => Items
                };
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}