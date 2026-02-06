namespace Maui.Controls.Sample;

public partial class VisualStateManagerCollectionViewPage : ContentPage
{
	public VisualStateManagerCollectionViewPage()
	{
		InitializeComponent();
		BindingContext = new VSMCollectionViewViewModel();
	}

	void OnCollectionSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!MyCollectionView.IsEnabled)
		{
			CVState.Text = "State: Disabled";
			return;
		}

		var hasSelection = e.CurrentSelection != null && e.CurrentSelection.Count > 0;
		CVState.Text = hasSelection ? $"State: Selected ({e.CurrentSelection.Count})" : "State: Normal";
	}

	void OnToggleCollectionViewDisabled(object sender, EventArgs e)
	{
		MyCollectionView.IsEnabled = !MyCollectionView.IsEnabled;
		CVToggleButton?.Text = MyCollectionView.IsEnabled ? "Disable" : "Enable";
		CVState.Text = MyCollectionView.IsEnabled
			? (GetSelectedCount() > 0 ? $"State: Selected ({GetSelectedCount()})" : "State: Normal")
			: "State: Disabled";
	}

	void OnResetCollectionView(object sender, EventArgs e)
	{
		MyCollectionView.IsEnabled = true; 
		MyCollectionView.SelectedItem = null;
		if (MyCollectionView.SelectionMode == SelectionMode.Multiple)
		{
			MyCollectionView.SelectedItems?.Clear();
		}
		CVToggleButton.Text = "Disable";
		CVState.Text = "State: Normal";
	}

	void OnItemPointerEntered(object sender, PointerEventArgs e)
	{
		if (!MyCollectionView.IsEnabled)
		{
			CVState.Text = "State: Disabled";
			return;
		}

		if (sender is VisualElement ve)
		{
			VisualStateManager.GoToState(ve, "PointerOver");
			CVState.Text = "State: PointerOver";
		}
	}

	void OnItemPointerExited(object sender, PointerEventArgs e)
	{
		if (sender is VisualElement ve)
		{
			// Restore based on selection (single or multiple)
			var bc = ve.BindingContext;
			var isSelected = IsItemSelected(bc);
			VisualStateManager.GoToState(ve, isSelected ? "Selected" : "Normal");

			if (!MyCollectionView.IsEnabled)
				CVState.Text = "State: Disabled";
			else
				CVState.Text = GetSelectedCount() > 0 ? $"State: Selected ({GetSelectedCount()})" : "State: Normal";
		}
	}

	int GetSelectedCount()
	{
		if (MyCollectionView.SelectionMode == SelectionMode.Multiple)
			return MyCollectionView.SelectedItems?.Count ?? 0;
		return MyCollectionView.SelectedItem != null ? 1 : 0;
	}

	bool IsItemSelected(object item)
	{
		if (item is null)
			return false;
		if (MyCollectionView.SelectionMode == SelectionMode.Multiple)
			return MyCollectionView.SelectedItems?.Contains(item) == true;
		return Equals(MyCollectionView.SelectedItem, item);

	}
}