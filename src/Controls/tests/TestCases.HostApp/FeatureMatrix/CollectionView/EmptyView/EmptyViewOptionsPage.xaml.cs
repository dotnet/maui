using System;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Maui.Controls.Sample.CollectionViewGalleries;

namespace Maui.Controls.Sample
{
    public partial class EmptyViewOptionsPage : ContentPage
    {
        private CollectionViewViewModel _viewModel;

        public EmptyViewOptionsPage(CollectionViewViewModel viewModel)
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
                    HorizontalOptions = LayoutOptions.Center,
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
                    HorizontalOptions = LayoutOptions.Center,
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
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
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
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        TextColor = Colors.Green
                    });
                    return grid;
                });
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
                    var label = new Label
                    {
                        Text =  "{Binding .}",
                    };

                    return label;
                });
            }
            else if (ItemTemplateGrid.IsChecked)
            {
                _viewModel.ItemTemplate = ExampleTemplates.PhotoTemplate();
                 
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