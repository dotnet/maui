using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class DragDropOptionsPage : ContentPage
{
	private readonly CollectionViewViewModel _viewModel;

	public DragDropOptionsPage(CollectionViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	void OnIsGroupedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (IsGroupedTrue.IsChecked)
			_viewModel.IsGrouped = true;
		else if (IsGroupedFalse.IsChecked)
			_viewModel.IsGrouped = false;
	}

	void OnCanReorderItemsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (CanReorderItemsTrue.IsChecked)
			_viewModel.CanReorderItems = true;
		else if (CanReorderItemsFalse.IsChecked)
			_viewModel.CanReorderItems = false;
	}

	void OnCanMixGroupsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (CanMixGroupsTrue.IsChecked)
			_viewModel.CanMixGroups = true;
		else if (CanMixGroupsFalse.IsChecked)
			_viewModel.CanMixGroups = false;
	}

	void OnItemsLayoutChanged(object sender, CheckedChangedEventArgs e)
	{
		if (ItemsLayoutVerticalList.IsChecked)
			_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
		else if (ItemsLayoutHorizontalList.IsChecked)
			_viewModel.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
		else if (ItemsLayoutVerticalGrid.IsChecked)
			_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
		else if (ItemsLayoutHorizontalGrid.IsChecked)
			_viewModel.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal);
	}

	void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is not RadioButton radioButton || !e.Value)
			return;

		if (radioButton == ItemsSourceObservableCollection)
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT;
		else if (radioButton == ItemsSourceGroupedList)
			_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT;
		else if (radioButton == ItemsSourceNone)
			_viewModel.ItemsSourceType = ItemsSourceType.None;
	}

	void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (FlowDirectionLeftToRight.IsChecked)
			_viewModel.FlowDirection = FlowDirection.LeftToRight;
		else if (FlowDirectionRightToLeft.IsChecked)
			_viewModel.FlowDirection = FlowDirection.RightToLeft;
	}
}
