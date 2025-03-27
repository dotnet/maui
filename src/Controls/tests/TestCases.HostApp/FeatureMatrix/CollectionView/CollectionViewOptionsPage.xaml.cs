using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Maui.Controls.Sample.CollectionViewGalleries;
 
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
                    TextColor = Colors.Blue,
                    AutomationId = "EmptyViewLabel"
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
                    TextColor = Colors.Blue,
                    AutomationId = "HeaderViewLabel"
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
                    TextColor = Colors.Red,
                    AutomationId = "FooterViewLabel"
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
                        TextColor = Colors.Blue,
                        AutomationId = "EmptyViewTemplateLabel"
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
                        TextColor = Colors.Blue,
                        AutomationId = "HeaderTemplateLabel"
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
                        TextColor = Colors.Green,
                        AutomationId = "FooterTemplateLabel"
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
                        TextColor = Colors.Green,
                        AutomationId = "GroupHeaderTemplateLabel"
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
                        TextColor = Colors.Red,
                        AutomationId = "GroupFooterTemplateLabel"
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
                _viewModel.ItemTemplate = new DataTemplate(() =>
                {
                     var label = new Label();
                     label.SetBinding(Label.TextProperty, new Binding("Caption"));
 
                    return label;
                });
            }
            else if (ItemTemplateGrid.IsChecked)
            {
                _viewModel.ItemTemplate = ExampleTemplates.PhotoTemplate();
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
            // Set ItemsSourceType based on selection
            if (radioButton == ItemsSourceList)
                _viewModel.ItemsSourceType = ItemsSourceType.ListT;
            else if (radioButton == ItemsSourceGroupedList)
                _viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
            else if (radioButton == ItemsSourceNone)
                _viewModel.ItemsSourceType = ItemsSourceType.None;
        }
    }
}
