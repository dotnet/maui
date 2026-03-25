namespace Maui.Controls.Sample;

public partial class ItemsSourceOptionsPage : ContentPage
{
	private CollectionViewViewModel _viewModel;

	public ItemsSourceOptionsPage(CollectionViewViewModel viewModel)
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
			_viewModel.ItemsSourceStringItems = false;
		}
		else if (StringItem.IsChecked)
		{
			_viewModel.ItemsSourceStringItems = true;
		}
	}
	private void OnItemsSourceChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton radioButton) || !e.Value)
			return;

		if (_viewModel.ItemsSourceStringItems == true)
		{
			if (radioButton == ItemsSourceObservableCollection)
				_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionStringT;
			else if (radioButton == ItemsSourceList)
				_viewModel.ItemsSourceType = ItemsSourceType.ListT;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.GroupedListStringT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType = ItemsSourceType.None;
			else if (radioButton == EmptyGroupedListT)
				_viewModel.ItemsSourceType = ItemsSourceType.EmptyGroupedListT;
			else if (radioButton == EmptyObservableCollectionT)
				_viewModel.ItemsSourceType = ItemsSourceType.EmptyObservableCollectionT;
		}
		else
		{
			if (radioButton == ItemsSourceObservableCollection)
				_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionModelT;
			else if (radioButton == ItemsSourceList)
				_viewModel.ItemsSourceType = ItemsSourceType.ListModelT;
			else if (radioButton == ItemsSourceGroupedList)
				_viewModel.ItemsSourceType = ItemsSourceType.GroupedListModelT;
			else if (radioButton == ItemsSourceNone)
				_viewModel.ItemsSourceType = ItemsSourceType.None;
			else if (radioButton == EmptyGroupedListT)
				_viewModel.ItemsSourceType = ItemsSourceType.EmptyGroupedListModelT;
			else if (radioButton == EmptyObservableCollectionT)
				_viewModel.ItemsSourceType = ItemsSourceType.EmptyObservableCollectionModelT;
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