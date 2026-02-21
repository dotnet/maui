using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class SelectionOptionsPage : ContentPage
	{
		private CollectionViewViewModel _viewModel;

		public SelectionOptionsPage(CollectionViewViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			_viewModel.SelectionChangedEventCount = 0;
			_viewModel.PreviousSelectionText = "No previous items";
			BindingContext = _viewModel;
		}

		private void ApplyButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
		private void OnSelectionModeButtonClicked(object sender, EventArgs e)
		{
			if (sender is Button button)
			{
				switch (button.AutomationId)
				{
					case "SelectionModeNone":
						_viewModel.SelectionMode = SelectionMode.None;
						break;
					case "SelectionModeSingle":
						_viewModel.SelectionMode = SelectionMode.Single;
						break;
					case "SelectionModeMultiple":
						_viewModel.SelectionMode = SelectionMode.Multiple;
						break;
					default:
						break;
				}
			}
		}

		private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
		{
			if (FlowDirectionLeftToRight.IsChecked)
			{
				_viewModel.FlowDirection = FlowDirection.LeftToRight;
			}
			else if (FlowDirectionRightToLeft.IsChecked)
			{
				_viewModel.FlowDirection = FlowDirection.RightToLeft;
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
				_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
			}
			else if (ItemsLayoutHorizontalGrid.IsChecked)
			{
				_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal);
			}
		}

		private void OnPreSelectionButtonClicked(object sender, EventArgs e)
		{
			if (sender is not Button button)
				return;

			var allItems = new List<CollectionViewViewModel.CollectionViewTestItem>();

			if (_viewModel.ItemsSourceType == ItemsSourceType.ObservableCollection5T &&
				_viewModel.ItemsSource is ObservableCollection<CollectionViewViewModel.CollectionViewTestItem> flatItems)
			{
				allItems = flatItems.ToList();
			}
			else if (_viewModel.ItemsSourceType == ItemsSourceType.GroupedListT &&
					 _viewModel.ItemsSource is List<Grouping<string, CollectionViewViewModel.CollectionViewTestItem>> groupedItems)
			{
				allItems = groupedItems.SelectMany(g => g).ToList();
			}

			var itemsToSelect = allItems.Where(item =>
				item.Caption == "Orange" ||
				item.Caption == "Apple" ||
				item.Caption == "Carrot" ||
				item.Caption == "Spinach").ToList();

			if (button.AutomationId == "SingleModePreselection" && _viewModel.SelectionMode == SelectionMode.Single)
			{
				_viewModel.SelectedItem = itemsToSelect.FirstOrDefault();
			}
			else if (button.AutomationId == "MultipleModePreselection" && _viewModel.SelectionMode == SelectionMode.Multiple)
			{
				_viewModel.SelectedItems = new ObservableCollection<object>(itemsToSelect.Cast<object>());
			}
		}

		private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!(sender is RadioButton radioButton) || !e.Value)
				return;
			else if (radioButton == ItemsSourceObservableCollection5)
				_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType = ItemsSourceType.None;
		}
	}
}