using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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
        ListT,
        EmptyListT,
        ObservableCollectionT,
        GroupedListT,
        EmptyGroupedListT,
        IEnumerableT
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
        private ItemsSourceType _itemsSourceType = ItemsSourceType.ListT;
        private bool _isGrouped;

        public event PropertyChangedEventHandler PropertyChanged;

        public CollectionViewViewModel()
        {
            LoadItems();
            ItemTemplate = ExampleTemplates.PhotoTemplate();
        }

        private List<CollectionViewTestItem> _flatList;
        private List<CollectionViewTestItem> _emptyList;
        private ObservableCollection<CollectionViewTestItem> _observableCollection;
        private List<Grouping<string, CollectionViewTestItem>> _groupedList;
        private List<Grouping<string, CollectionViewTestItem>> _emptyGroupedList;

        private void LoadItems()
        {
            // Initialize non-grouped lists
            _flatList = new List<CollectionViewTestItem>();
            AddItems(_flatList, 4);

            _emptyList = new List<CollectionViewTestItem>();
            _observableCollection = new ObservableCollection<CollectionViewTestItem>(_flatList);

            // Initialize grouped lists
            _groupedList = new List<Grouping<string, CollectionViewTestItem>>
            {
                new Grouping<string, CollectionViewTestItem>("Group A", new List<CollectionViewTestItem>()),
                new Grouping<string, CollectionViewTestItem>("Group B", new List<CollectionViewTestItem>())
            };
            AddItems(_groupedList[0], 2);
            AddItems(_groupedList[1], 2);

            _emptyGroupedList = new List<Grouping<string, CollectionViewTestItem>>();
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
                    OnPropertyChanged(nameof(ItemsSource)); // Refresh ItemsSource when grouped changes
                }
            }
        }

        public object ItemsSource
        {
            get
            {
                if (IsGrouped)
                {
                    return ItemsSourceType switch
                    {
                        ItemsSourceType.GroupedListT => _groupedList,
                        ItemsSourceType.EmptyGroupedListT => _emptyGroupedList,
                        _ => _groupedList // Default to grouped list
                    };
                }
                else
                {
                    return ItemsSourceType switch
                    {
                        ItemsSourceType.ListT => _flatList,
                        ItemsSourceType.EmptyListT => _emptyList,
                        ItemsSourceType.ObservableCollectionT => _observableCollection,
                        ItemsSourceType.IEnumerableT => _flatList.AsEnumerable(),
                        ItemsSourceType.None => null,
                        _ => _flatList // Default to flat list
                    };
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddItems(IList<CollectionViewTestItem> list, int count)
        {
            string[] images =
            {
                "cover1.jpg",
                "oasis.jpg",
                "photo.jpg",
                "Vegetables.jpg",
                "Fruits.jpg",
                "FlowerBuds.jpg",
                "Legumes.jpg"
            };

            for (int n = 0; n < count; n++)
            {
                list.Add(new CollectionViewTestItem(
                    $"{images[n % images.Length]}, {n}", images[n % images.Length], n));
            }
        }

        public class CustomDataTemplateSelector : DataTemplateSelector
        {
            public DataTemplate Template1 { get; set; }
            public DataTemplate Template2 { get; set; }

            protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
            {
                // Implement your logic to select the appropriate template
                if (item is CollectionViewTestItem testItem)
                {
                    return testItem.Index % 2 == 0 ? Template1 : Template2;
                }

                return Template1;
            }
        }
    }

    public class CollectionViewTestItem
    {
        public string Caption { get; set; }
        public string Image { get; set; }
        public int Index { get; set; }

        public CollectionViewTestItem(string caption, string image, int index)
        {
            Caption = caption;
            Image = image;
            Index = index;
        }
    }

    internal class ExampleTemplates
    {
        public static DataTemplate PhotoTemplate()
        {
            return new DataTemplate(() =>
            {
                var templateLayout = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition() },
                    WidthRequest = 200,
                    HeightRequest = 100,
                };

                var image = new Image
                {
                    WidthRequest = 100,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(2, 5, 2, 2),
                    AutomationId = "photo"
                };
                image.SetBinding(Image.SourceProperty, new Binding("Image"));

                var caption = new Label
                {
                    FontSize = 12,
                    HorizontalOptions = LayoutOptions.Fill,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(2, 0, 2, 2),
                    BackgroundColor = Colors.Blue
                };
                caption.SetBinding(Label.TextProperty, new Binding("Caption"));

                templateLayout.Children.Add(image);
                templateLayout.Children.Add(caption);
                Grid.SetRow(caption, 1);

                return templateLayout;
            });
        }
    }
}