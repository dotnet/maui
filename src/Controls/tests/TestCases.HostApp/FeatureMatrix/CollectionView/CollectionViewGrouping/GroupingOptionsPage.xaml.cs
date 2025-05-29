using System;
using System.Collections.ObjectModel;
using Maui.Controls.Sample.CollectionViewGalleries;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class GroupingOptionsPage : ContentPage
{
	private CollectionViewViewModel _viewModel;
	public GroupingOptionsPage(CollectionViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}
	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
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
					Text = "GroupHeaderTemplate",
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
					Text = "GroupFooterTemplate",
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
			_viewModel.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Caption"));
				return label;
			});
		}
	}

	private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;
		if (radioButton == ItemsSourceObservableCollection)
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT;
		else if (radioButton == ItemsSourceGroupedList)
			_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
		else if (radioButton == ItemsSourceNone)
			_viewModel.ItemsSourceType = ItemsSourceType.None;
	}

	private void OnCanMixGroupsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (CanMixGroupsTrue.IsChecked)
		{
			_viewModel.CanMixGroups = true;
		}
		else if (CanMixGroupsFalse.IsChecked)
		{
			_viewModel.CanMixGroups = false;
		}
	}

	private void OnCanReorderItemsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (CanReorderItemsTrue.IsChecked)
		{
			_viewModel.CanReorderItems = true;
		}
		else if (CanReorderItemsFalse.IsChecked)
		{
			_viewModel.CanReorderItems = false;
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
}