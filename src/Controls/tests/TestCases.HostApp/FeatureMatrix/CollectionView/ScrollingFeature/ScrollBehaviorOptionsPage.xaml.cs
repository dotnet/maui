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