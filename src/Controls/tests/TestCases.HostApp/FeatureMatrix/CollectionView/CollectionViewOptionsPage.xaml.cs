using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
 
namespace Maui.Controls.Sample
{
    public partial class CollectionViewOptionsPage : ContentPage
    {
        private CollectionViewViewModel _viewModel;
 
        public CollectionViewOptionsPage(CollectionViewViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
 
        private void ApplyButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
 
        private void OnEmptyViewChanged(object sender, CheckedChangedEventArgs e)
        {
            if (EmptyViewNone.IsChecked)
            {
                _viewModel.EmptyView = null;
            }
            else if (EmptyViewString.IsChecked)
            {
                _viewModel.EmptyView = "No Items Available(String)";
            }
            else if (EmptyViewGrid.IsChecked)
            {
                Grid grid = new Grid
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(10)
                };
                grid.Children.Add(new Label
                {
                    Text = "No Items Available(Grid View)",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 18,
                    TextColor = Colors.Blue
                });
                _viewModel.EmptyView = grid;
            }
        }
 
        private void OnHeaderChanged(object sender, CheckedChangedEventArgs e)
        {
            if (HeaderNone.IsChecked)
            {
                _viewModel.Header = null;
            }
            else if (HeaderString.IsChecked)
            {
                _viewModel.Header = "CollectionView Header(String)";
            }
            else if (HeaderGrid.IsChecked)
            {
                Grid grid = new Grid
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(10)
                };
                grid.Children.Add(new Label
                {
                    Text = "CollectionView Header(Grid View)",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Blue
                });
                _viewModel.Header = grid;
            }
        }
 
        private void OnFooterChanged(object sender, CheckedChangedEventArgs e)
        {
            if (FooterNone.IsChecked)
            {
                _viewModel.Footer = null;
            }
            else if (FooterString.IsChecked)
            {
                _viewModel.Footer = "CollectionView Footer(String)";
            }
            else if (FooterGrid.IsChecked)
            {
                Grid grid = new Grid
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(10)
                };
                grid.Children.Add(new Label
                {
                    Text = "CollectionView Footer(Grid View)",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Red
                });
                _viewModel.Footer = grid;
            }
        }
 
        private void OnEmptyViewTemplateChanged(object sender, CheckedChangedEventArgs e)
        {
            if (EmptyViewTemplateNone.IsChecked)
            {
                _viewModel.EmptyViewTemplate = null;
            }
            else if (EmptyViewTemplateGrid.IsChecked)
            {
                _viewModel.EmptyViewTemplate = new DataTemplate(() =>
                {
                    Grid grid = new Grid
                    {
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
                    grid.Children.Add(new Label
                    {
                        Text = "No Template Items Available(Grid View)",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 18,
                        TextColor = Colors.Blue
                    });
                    return grid;
                });
            }
        }
 
        private void OnHeaderTemplateChanged(object sender, CheckedChangedEventArgs e)
        {
            if (HeaderTemplateNone.IsChecked)
            {
                _viewModel.HeaderTemplate = null;
            }
            else if (HeaderTemplateGrid.IsChecked)
            {
                _viewModel.HeaderTemplate = new DataTemplate(() =>
                {
                    Grid grid = new Grid
                    {
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
                    grid.Children.Add(new Label
                    {
                        Text = "Header Template(Grid View)",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Blue
                    });
                    return grid;
                });
            }
        }
 
        private void OnFooterTemplateChanged(object sender, CheckedChangedEventArgs e)
        {
            if (FooterTemplateNone.IsChecked)
            {
                _viewModel.FooterTemplate = null;
            }
            else if (FooterTemplateGrid.IsChecked)
            {
                _viewModel.FooterTemplate = new DataTemplate(() =>
                {
                    Grid grid = new Grid
                    {
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
                    grid.Children.Add(new Label
                    {
                        Text = "Footer Template(Grid View)",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Green
                    });
                    return grid;
                });
            }
        }
 
        private void OnGroupHeaderTemplateChanged(object sender, CheckedChangedEventArgs e)
        {
            if (GroupHeaderTemplateNone.IsChecked)
            {
                _viewModel.GroupHeaderTemplate = null;
            }
            else if (GroupHeaderTemplateGrid.IsChecked)
            {
                _viewModel.GroupHeaderTemplate = new DataTemplate(() =>
                {
                    Grid grid = new Grid
                    {
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
                    grid.Children.Add(new Label
                    {
                        Text = "Group Header Template(Grid View)",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Green
                    });
                    return grid;
                });
            }
        }
        private void OnGroupFooterTemplateChanged(object sender, CheckedChangedEventArgs e)
        {
            if (GroupFooterTemplateNone.IsChecked)
            {
                _viewModel.GroupFooterTemplate = null;
            }
            else if (GroupFooterTemplateGrid.IsChecked)
            {
                _viewModel.GroupFooterTemplate = new DataTemplate(() =>
                {
                    Grid grid = new Grid
                    {
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
                    grid.Children.Add(new Label
                    {
                        Text = "Group Footer Template(Grid View)",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Red
                    });
                    return grid;
                });
            }
        }
        private void OnIsGroupedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (IsGroupedFalse.IsChecked)
            {
                _viewModel.IsGrouped = false;
            }
            else if (IsGroupedTrue.IsChecked)
            {
                _viewModel.IsGrouped = true;
            }
        }
 
        private void OnItemTemplateChanged(object sender, CheckedChangedEventArgs e)
        {
            if (ItemTemplateNone.IsChecked)
            {
                _viewModel.ItemTemplate = null;
            }
            else if (ItemTemplateBasic.IsChecked)
            {
                // Basic DataTemplate with a single Label element
                _viewModel.ItemTemplate = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        Text = "CollectionViewItem",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Black,
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
 
                    return label;
                });
            }
            else if (ItemTemplateGrid.IsChecked)
            {
                // Grid DataTemplate with proper binding
                _viewModel.ItemTemplate = new DataTemplate(() =>
                {
                    var captionLabel = new Label { FontSize = 18, HorizontalOptions = LayoutOptions.Center };
                    captionLabel.SetBinding(Label.TextProperty, new Binding("Caption"));
                    var image = new Image { WidthRequest = 100, HorizontalOptions = LayoutOptions.Center };
                    image.SetBinding(Image.SourceProperty, new Binding("Image"));
                    var grid = new Grid
                    {
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid.Children.Add(captionLabel);
                    Grid.SetRow(captionLabel, 0);
                    grid.Children.Add(image);
                    Grid.SetRow(image, 1);
                    return grid;
                });
            }
            else if (ItemTemplateSelector.IsChecked)
            {
                // DataTemplateSelector
                var template1 = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        Text = "Template 1",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Black,
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
 
                    return label;
                });
 
                var template2 = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        Text = "Template 2",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Colors.Black,
                        BackgroundColor = Colors.LightGray,
                        Padding = new Thickness(10)
                    };
 
                    return label;
                });
 
                _viewModel.ItemTemplate = new CollectionViewViewModel.CustomDataTemplateSelector
                {
                    Template1 = template1,
                    Template2 = template2
                };
            }
        }
 
        private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!(sender is RadioButton radioButton) || !e.Value)
                return;
            // Set IsGrouped based on selection
            if (radioButton == ItemsSourceGroupedList || radioButton == ItemsSourceEmptyGroupedList)
            {
                _viewModel.IsGrouped = true;
            }
            else
            {
                _viewModel.IsGrouped = false;
            }
            // Set ItemsSourceType based on selection
            if (radioButton == ItemsSourceList)
                _viewModel.ItemsSourceType = ItemsSourceType.ListT;
            else if (radioButton == ItemsSourceEmptyList)
                _viewModel.ItemsSourceType = ItemsSourceType.EmptyListT;
            else if (radioButton == ItemsSourceObservableCollection)
                _viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT;
            else if (radioButton == ItemsSourceGroupedList)
                _viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
            else if (radioButton == ItemsSourceEmptyGroupedList)
                _viewModel.ItemsSourceType = ItemsSourceType.EmptyGroupedListT;
            else if (radioButton == ItemsSourceIEnumerable)
                _viewModel.ItemsSourceType = ItemsSourceType.IEnumerableT;
            else if (radioButton == ItemsSourceNone)
                _viewModel.ItemsSourceType = ItemsSourceType.None;
        }
    }
}
