using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Maui.Controls.Sample.CollectionViewGalleries;

namespace Maui.Controls.Sample
{
    public class Grouping<TKey, TItem> : ObservableCollection<TItem>
    {
        public TKey Key { get; }

        public Grouping(TKey key, IEnumerable<TItem> items) : base(items)
        {
            Key = key;
        }
    }

    public enum ItemsSourceType
    {
        None,
        ObservableCollectionT,
        GroupedListT,
    }

    public class CollectionViewViewModel : INotifyPropertyChanged
    {
        private object _emptyView;
        private object _header;
        private object _footer;
        private DataTemplate _emptyViewTemplate;
        private DataTemplate _headerTemplate;
        private DataTemplate _footerTemplate;
        private DataTemplate _groupHeaderTemplate;
        private DataTemplate _groupFooterTemplate;
        private DataTemplate _itemTemplate;
        private IItemsLayout _itemsLayout = LinearItemsLayout.Vertical;
        private ItemsSourceType _itemsSourceType = ItemsSourceType.None;
        private bool _isGrouped = false;
        private bool _canReorderItems = false;
        private bool _canMixGroups = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public CollectionViewViewModel()
        {
            LoadItems();
            ItemTemplate = new DataTemplate(() =>
            {
                var stackLayout = new StackLayout
                {
                    Padding = new Thickness(10),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                var label = new Label
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                label.SetBinding(Label.TextProperty, "Caption");
                stackLayout.Children.Add(label);
                return stackLayout;
            });

            GroupHeaderTemplate = new DataTemplate(() =>
            {
                var stackLayout = new StackLayout
                {
                    BackgroundColor = Colors.LightGray
                };
                var label = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18
                };
                label.SetBinding(Label.TextProperty, "Key");
                stackLayout.Children.Add(label);
                return stackLayout;
            });
        }

        private ObservableCollection<CollectionViewTestItem> _observableCollection;
        private List<Grouping<string, CollectionViewTestItem>> _groupedList;


        private void LoadItems()
        {
            _observableCollection = new ObservableCollection<CollectionViewTestItem>();
            AddItems(_observableCollection, 7, "Fruits");
            AddItems(_observableCollection, 7, "Vegetables");

            _groupedList = new List<Grouping<string, CollectionViewTestItem>>
            {
                new Grouping<string, CollectionViewTestItem>("Fruits", new List<CollectionViewTestItem>()),
                new Grouping<string, CollectionViewTestItem>("Vegetables", new List<CollectionViewTestItem>())
            };

            AddItems(_groupedList[0], 4, "Fruits");
            AddItems(_groupedList[1], 4, "Vegetables");
        }


        public object EmptyView
        {
            get => _emptyView;
            set { _emptyView = value; OnPropertyChanged(); }
        }

        public object Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(); }
        }

        public object Footer
        {
            get => _footer;
            set { _footer = value; OnPropertyChanged(); }
        }

        public DataTemplate EmptyViewTemplate
        {
            get => _emptyViewTemplate;
            set { _emptyViewTemplate = value; OnPropertyChanged(); }
        }

        public DataTemplate HeaderTemplate
        {
            get => _headerTemplate;
            set { _headerTemplate = value; OnPropertyChanged(); }
        }

        public DataTemplate FooterTemplate
        {
            get => _footerTemplate;
            set { _footerTemplate = value; OnPropertyChanged(); }
        }

        public DataTemplate GroupHeaderTemplate
        {
            get => _groupHeaderTemplate;
            set { _groupHeaderTemplate = value; OnPropertyChanged(); }
        }

        public DataTemplate GroupFooterTemplate
        {
            get => _groupFooterTemplate;
            set { _groupFooterTemplate = value; OnPropertyChanged(); }
        }

        public DataTemplate ItemTemplate
        {
            get => _itemTemplate;
            set { _itemTemplate = value; OnPropertyChanged(); }
        }

        public ItemsSourceType ItemsSourceType
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

        public bool IsGrouped
        {
            get => _isGrouped;
            set
            {
                if (_isGrouped != value)
                {
                    _isGrouped = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanMixGroups
        {
            get => _canMixGroups;
            set
            {
                if (_canMixGroups != value)
                {
                    _canMixGroups = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanReorderItems
        {
            get => _canReorderItems;
            set
            {
                if (_canReorderItems != value)
                {
                    _canReorderItems = value;
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
                    ItemsSourceType.GroupedListT => _groupedList,
                    ItemsSourceType.ObservableCollectionT => _observableCollection,
                    ItemsSourceType.None => null,
                    _ => null

                };
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == nameof(IsGrouped))
            {
                OnPropertyChanged(nameof(ItemsSource));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddItems(IList<CollectionViewTestItem> list, int count, string category)
        {
            string[] fruits =
            {
                "Apple", "Banana", "Orange", "Grapes", "Mango",
                "Pineapple", "Strawberry", "Blueberry", "Peach", "Cherry",
                "Watermelon", "Papaya", "Kiwi", "Pear", "Plum",
                "Avocado", "Fig", "Guava", "Lychee", "Pomegranate",
                "Lime", "Lemon", "Coconut", "Apricot", "Blackberry"
            };

            string[] vegetables =
            {
                "Carrot", "Broccoli", "Spinach", "Potato", "Tomato",
                "Cucumber", "Lettuce", "Onion", "Garlic", "Pepper",
                "Zucchini", "Pumpkin", "Radish", "Beetroot", "Cabbage",
                "Sweet Potato", "Turnip", "Cauliflower", "Celery", "Asparagus",
                "Eggplant", "Chili", "Corn", "Peas", "Mushroom"
            };

            string[] items = category == "Fruits" ? fruits : vegetables;

            for (int n = 0; n < count; n++)
            {
                list.Add(new CollectionViewTestItem(items[n % items.Length]));
            }
        }

        public class CollectionViewTestItem
        {
            public string Caption { get; set; }

            public CollectionViewTestItem(string caption)
            {
                Caption = caption;
            }
        }
    }
}