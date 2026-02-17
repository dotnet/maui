using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29141, "[iOS] Group Header/Footer Repeated for All Items When IsGrouped is True for ObservableCollection in CollectionView", PlatformAffected.Android)]
public class Issue29141 : ContentPage
{
    Issue29141CollectionViewViewModel _viewModel;
    CollectionView _collectionView;
    RadioButton _groupHeaderTemplateNone;
    RadioButton _groupHeaderTemplateGrid;
    RadioButton _groupFooterTemplateNone;
    RadioButton _groupFooterTemplateGrid;
    RadioButton _isGroupedFalse;
    RadioButton _isGroupedTrue;

    public Issue29141()
    {
        _viewModel = new Issue29141CollectionViewViewModel();
        BindingContext = _viewModel;

        // Create CollectionView
        _collectionView = new CollectionView
        {
            AutomationId = "collectionView"
        };
        _collectionView.SetBinding(CollectionView.ItemsSourceProperty, "ItemsSource");
        _collectionView.SetBinding(CollectionView.ItemTemplateProperty, "ItemTemplate");
        _collectionView.SetBinding(CollectionView.IsGroupedProperty, "IsGrouped");
        _collectionView.SetBinding(CollectionView.GroupHeaderTemplateProperty, "GroupHeaderTemplate");
        _collectionView.SetBinding(CollectionView.GroupFooterTemplateProperty, "GroupFooterTemplate");

        // Create GroupHeaderTemplate radio buttons
        _groupHeaderTemplateNone = new RadioButton
        {
            IsChecked = true,
            Content = "None",
            FontSize = 11,
            GroupName = "GroupHeaderTemplateGroup",
            AutomationId = "GroupHeaderTemplateNone"
        };
        _groupHeaderTemplateNone.CheckedChanged += OnGroupHeaderTemplateChanged;

        _groupHeaderTemplateGrid = new RadioButton
        {
            Content = "View",
            FontSize = 11,
            GroupName = "GroupHeaderTemplateGroup",
            AutomationId = "GroupHeaderTemplateGrid"
        };
        _groupHeaderTemplateGrid.CheckedChanged += OnGroupHeaderTemplateChanged;

        // Create GroupFooterTemplate radio buttons
        _groupFooterTemplateNone = new RadioButton
        {
            IsChecked = true,
            Content = "None",
            FontSize = 11,
            GroupName = "GroupFooterTemplateGroup",
            AutomationId = "GroupFooterTemplateNone"
        };
        _groupFooterTemplateNone.CheckedChanged += OnGroupFooterTemplateChanged;

        _groupFooterTemplateGrid = new RadioButton
        {
            Content = "View",
            FontSize = 11,
            GroupName = "GroupFooterTemplateGroup",
            AutomationId = "GroupFooterTemplateGrid"
        };
        _groupFooterTemplateGrid.CheckedChanged += OnGroupFooterTemplateChanged;

        // Create IsGrouped radio buttons
        _isGroupedFalse = new RadioButton
        {
            Content = "False",
            IsChecked = true,
            FontSize = 11,
            AutomationId = "IsGroupedFalse"
        };
        _isGroupedFalse.CheckedChanged += OnIsGroupedChanged;

        _isGroupedTrue = new RadioButton
        {
            Content = "True",
            FontSize = 11,
            AutomationId = "IsGroupedTrue"
        };
        _isGroupedTrue.CheckedChanged += OnIsGroupedChanged;

        // Build the UI
        var controlsLayout = new StackLayout
        {
            Padding = new Thickness(10),
            Spacing = 10,
            Children =
            {
                new Label
                {
                    Text = "GroupHeaderTemplate:",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 12
                },
                new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { _groupHeaderTemplateNone, _groupHeaderTemplateGrid }
                },
                new Label
                {
                    Text = "GroupFooterTemplate:",
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 12
                },
                new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { _groupFooterTemplateNone, _groupFooterTemplateGrid }
                },
                new Label
                {
                    Text = "IsGrouped:",
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold
                },
                new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { _isGroupedFalse, _isGroupedTrue }
                }
            }
        };

        var scrollView = new ScrollView
        {
            Content = controlsLayout
        };

        Content = new Grid
        {
            Padding = new Thickness(10),
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            Children =
            {
                _collectionView,
                scrollView
            }
        };

        Grid.SetRow(_collectionView, 0);
        Grid.SetRow(scrollView, 1);
    }

    void OnGroupHeaderTemplateChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_groupHeaderTemplateNone.IsChecked)
        {
            _viewModel.GroupHeaderTemplate = null;
        }
        else if (_groupHeaderTemplateGrid.IsChecked)
        {
            _viewModel.GroupHeaderTemplate = new DataTemplate(() =>
            {
                return new Grid
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(10),
                    Children =
                    {
                            new Label
                            {
                                Text = "Group Header Template (Grid View)",
                                FontSize = 18,
                                AutomationId = "GroupHeaderTemplate",
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                TextColor = Colors.Green
                            }
                    }
                };
            });
        }
    }

    void OnGroupFooterTemplateChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_groupFooterTemplateNone.IsChecked)
        {
            _viewModel.GroupFooterTemplate = null;
        }
        else if (_groupFooterTemplateGrid.IsChecked)
        {
            _viewModel.GroupFooterTemplate = new DataTemplate(() =>
            {
                return new Grid
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(10),
                    Children =
                    {
                            new Label
                            {
                                Text = "Group Footer Template (Grid View)",
                                FontSize = 18,
                                AutomationId = "GroupFooterTemplate",
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center,
                                TextColor = Colors.Red
                            }
                    }
                };
            });
        }
    }

    void OnIsGroupedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isGroupedFalse.IsChecked)
        {
            _viewModel.IsGrouped = false;
        }
        else if (_isGroupedTrue.IsChecked)
        {
            _viewModel.IsGrouped = true;
        }
    }
}

public class Issue29141CollectionViewViewModel : INotifyPropertyChanged
{
    DataTemplate _groupHeaderTemplate;
    DataTemplate _groupFooterTemplate;
    DataTemplate _itemTemplate;
    bool _isGrouped = false;
    ObservableCollection<Issue29141ItemModel> _observableCollection;

    public event PropertyChangedEventHandler PropertyChanged;

    public Issue29141CollectionViewViewModel()
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

    void LoadItems()
    {
        _observableCollection = new ObservableCollection<Issue29141ItemModel>
            {
                new Issue29141ItemModel { Caption = "Item 1" },
                new Issue29141ItemModel { Caption = "Item 2" },
                new Issue29141ItemModel { Caption = "Item 3" }
            };
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

    public bool IsGrouped
    {
        get => _isGrouped;
        set { _isGrouped = value; OnPropertyChanged(); }
    }

    public object ItemsSource
    {
        get => _observableCollection;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (propertyName == nameof(IsGrouped))
        {
            OnPropertyChanged(nameof(ItemsSource));
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal class Issue29141Grouping<TKey, TItem> : List<TItem>
{
    public TKey Key { get; }

    public Issue29141Grouping(TKey key, List<TItem> items) : base(items)
    {
        Key = key;
    }

    public override string ToString()
    {
        return Key?.ToString() ?? base.ToString();
    }
}

internal class Issue29141ItemModel
{
    public string Caption { get; set; }

    public override string ToString()
    {
        return !string.IsNullOrEmpty(Caption) ? Caption : base.ToString();
    }
}
