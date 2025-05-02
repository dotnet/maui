namespace Maui.Controls.Sample;

public partial class ItemsSourceOptionsPage : ContentPage
{
	private ItemsSourceViewModel _viewModel;

	public ItemsSourceOptionsPage(ItemsSourceViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}
	private void OnItemSourceTypeChanged(object sender, EventArgs e)
	{
		if (ModelItem.IsChecked)
		{
			_viewModel.ItemsSourceModelItems = true;
			_viewModel.ItemsSourceStringItems = false;
		}
		else if (StringItem.IsChecked)
		{
			_viewModel.ItemsSourceStringItems = true;
			_viewModel.ItemsSourceModelItems = false;
		}
	}
	private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;

		if (_viewModel.ItemsSourceStringItems == true && _viewModel.ItemsSourceModelItems == false)
		{
			if (radioButton == ItemsSourceObservableCollection)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.ObservableCollectionT;
			else if (radioButton == ItemsSourceList)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.ListT;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.GroupedListT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.None;
			else if (radioButton == EmptyGroupedListT)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.EmptyGroupedListT;
			else if (radioButton == EmptyObservableCollectionT)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.EmptyObservableCollectionT;
		}
		else
		{
			if (radioButton == ItemsSourceObservableCollection)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.ObservableCollectionModelT;
			else if (radioButton == ItemsSourceList)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.ListModelT;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.GroupedListModelT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.None;
			else if (radioButton == EmptyGroupedListT)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.EmptyGroupedListModelT;
			else if (radioButton == EmptyObservableCollectionT)
				_viewModel.ItemsSourceType1 = ItemsSourceType1.EmptyObservableCollectionModelT;
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
}