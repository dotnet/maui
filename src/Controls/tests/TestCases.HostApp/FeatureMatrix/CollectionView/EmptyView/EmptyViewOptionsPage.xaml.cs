using System;
using System.Collections.ObjectModel;
using Maui.Controls.Sample.CollectionViewGalleries;
using Microsoft.Maui.Controls;

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
					Padding = new Thickness(10),
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
			else if (EmptyViewCustomSize.IsChecked)
			{
				Frame customView = new Frame
				{
					BackgroundColor = Colors.LightBlue,
					HeightRequest = 200,
					AutomationId = "EmptyViewLabel",
					WidthRequest = 300,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Content = new Label
					{
						Text = "Custom Empty View (Sized)",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
					}
				};
				_viewModel.EmptyView = customView;
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
			else if (EmptyViewTemplateCustomSize.IsChecked)
			{
				_viewModel.EmptyViewTemplate = new DataTemplate(() =>
				{
					Frame customView = new Frame
					{
						BackgroundColor = Colors.LightBlue,
						HeightRequest = 200,
						BorderColor = Colors.Green,
						AutomationId = "EmptyViewTemplateLabel",
						WidthRequest = 300,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						Content = new Label
						{
							Text = "Custom EmptyViewTemplate (Sized)",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
						}
					};
					return customView;
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
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Caption"));
					return label;
				});
			}
			else if (ItemTemplateGrid.IsChecked)
			{
				_viewModel.ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						ColumnDefinitions =
						{
							 new ColumnDefinition { Width = GridLength.Star },
							 new ColumnDefinition { Width = GridLength.Star }
						},
						Padding = new Thickness(10),
						BackgroundColor = Colors.LightBlue

					};

					var label1 = new Label
					{
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Start,
						FontAttributes = FontAttributes.Bold,
						TextColor = Colors.Green
					};
					label1.SetBinding(Label.TextProperty, "Caption");
					grid.Children.Add(label1);
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

		private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!(sender is RadioButton radioButton) || !e.Value)
				return;
			// Set IsGrouped based on selection

			if (radioButton == ItemsSourceObservableCollection5)
				_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
			else if (radioButton == ItemsSourceEmptyObservableCollection)
				_viewModel.ItemsSourceType = ItemsSourceType.EmptyObservableCollectionT;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
			else if (radioButton == ItemsSourceEmptyGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.EmptyGroupedListT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType = ItemsSourceType.None;
		}

		private void OnItemsLayoutChanged(object sender, CheckedChangedEventArgs e)
		{
			if (ItemsLayoutVerticalList.IsChecked)
			{
				_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
			}
			else if (ItemsLayoutHorizontalList.IsChecked)
			{
				_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
			}
			else if (ItemsLayoutVerticalGrid.IsChecked)
			{
				_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical); // 2 columns
			}
			else if (ItemsLayoutHorizontalGrid.IsChecked)
			{
				_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal); // 2 rows
			}
		}
	}
}