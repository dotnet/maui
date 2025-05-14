using System;
using System.Collections.ObjectModel;
using Maui.Controls.Sample.CollectionViewGalleries;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class HeaderFooterOptionsPage : ContentPage
	{
		private CollectionViewViewModel _viewModel;

		public HeaderFooterOptionsPage(CollectionViewViewModel viewModel)
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

		private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!(sender is RadioButton radioButton) || !e.Value)
				return;
			if (radioButton == ItemsSourceObservableCollection25)
				_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection25T;
			else if (radioButton == ItemsSourceObservableCollection5)
				_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType = ItemsSourceType.None;
		}
	}
}
