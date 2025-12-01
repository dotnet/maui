namespace Maui.Controls.Sample;

public partial class ScrollBehaviorOptionsPage : ContentPage
{

	private CollectionViewViewModel _viewModel;
	public ScrollBehaviorOptionsPage(CollectionViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}
	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;
		if (radioButton == ItemsSourceObservableCollection3)
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT3;
		else if (radioButton == ItemsSourceObservableCollection2)
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT2;
		else if (radioButton == ItemsSourceGroupedList2)
			_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT2;
		else if (radioButton == ItemsSourceGroupedList3)
			_viewModel.ItemsSourceType = ItemsSourceType.GroupedListT3;
		else if (radioButton == ItemsSourceNone)
			_viewModel.ItemsSourceType = ItemsSourceType.None;
	}

	private void OnItemSizingStrategyChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;

		if (radioButton == ItemSizingMeasureFirstItem)
			_viewModel.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
		else if (radioButton == ItemSizingMeasureAllItems)
			_viewModel.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
	}

	private void OnItemsUpdatingScrollModeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;

		if (radioButton == ItemsUpdatingKeepItemsInView)
			_viewModel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;
		else if (radioButton == ItemsUpdatingKeepLastItemInView)
			_viewModel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
		else if (radioButton == ItemsUpdatingKeepScrollOffset)
			_viewModel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
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

	private void OnGroupNameChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;

		if (radioButton == FruitGroup)
		{
			_viewModel.GroupName = "Fruits";
		}
		else if (radioButton == VegetableGroup)
		{
			_viewModel.GroupName = "Vegetables";
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

	private void OnScrollToPositionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;

		if (radioButton == ScrollToPositionMakeVisible)
			_viewModel.ScrollToPosition = ScrollToPosition.MakeVisible;
		else if (radioButton == ScrollToPositionStart)
			_viewModel.ScrollToPosition = ScrollToPosition.Start;
		else if (radioButton == ScrollToPositionCenter)
			_viewModel.ScrollToPosition = ScrollToPosition.Center;
		else if (radioButton == ScrollToPositionEnd)
			_viewModel.ScrollToPosition = ScrollToPosition.End;
	}

	private void ScrollToIndexItemChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;
		if (radioButton == ScrollToByIndex)
		{
			_viewModel.ScrollToByIndexOrItem = "Index";
		}
		else if (radioButton == ScrollToByItem)
		{
			_viewModel.ScrollToByIndexOrItem = "Item";
		}
	}

	private void ScrollToIndexEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (int.TryParse(ScrollToIndexEntry.Text, out int index))
		{
			_viewModel.ScrollToIndex = index;
		}
	}

	private void GroupIndexEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (int.TryParse(GroupIndexEntry.Text, out int groupIndex))
		{
			_viewModel.GroupIndex = groupIndex;
		}
	}

	private void ScrollToItemEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		_viewModel.ScrollToItem = ScrollToItemEntry.Text;
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
}