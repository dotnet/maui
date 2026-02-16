using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public partial class VisualStateManagerCollectionViewPage : ContentPage
{
	public VisualStateManagerCollectionViewPage()
	{
		InitializeComponent();
		BindingContext = new VSMCollectionViewViewModel();
		VisualStateManager.GoToState(MyCollectionView, "Normal");
	}

	void OnCollectionSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!MyCollectionView.IsEnabled)
		{
			CVState.Text = "State: Disabled";
			return;
		}

		var hasSelection = e.CurrentSelection != null && e.CurrentSelection.Count > 0;
		VisualStateManager.GoToState(MyCollectionView, hasSelection ? "Selected" : "Normal");
		CVState.Text = hasSelection ? $"State: Selected ({e.CurrentSelection.Count})" : "State: Normal";
	}

	void OnToggleCollectionViewDisabled(object sender, EventArgs e)
	{
		MyCollectionView.IsEnabled = !MyCollectionView.IsEnabled;
		CVToggleButton.Text = MyCollectionView.IsEnabled ? "Disable" : "Enable";

		if (!MyCollectionView.IsEnabled)
		{
			VisualStateManager.GoToState(MyCollectionView, "Disabled");
			CVState.Text = "State: Disabled";
		}
		else
		{
			VisualStateManager.GoToState(MyCollectionView, "Normal");
			CVState.Text = "State: Normal";
		}
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
		VisualStateManager.GoToState(MyCollectionView, "Normal");
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
		if (!MyCollectionView.IsEnabled)
		{
			CVState.Text = "State: Disabled";
			return;
		}

		if (sender is VisualElement ve)
		{
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

	void OnSelectItem(object sender, EventArgs e)
	{
		if (!MyCollectionView.IsEnabled)
		{
			CVState.Text = "State: Disabled";
			return;
		}
		if (MyCollectionView.ItemsSource is ObservableCollection<Item> items && items.Count > 0)
		{
			if (MyCollectionView.SelectionMode == SelectionMode.Multiple)
			{
				if (MyCollectionView.SelectedItems?.Contains(items[0]) != true)
				{
					MyCollectionView.SelectedItems?.Add(items[0]);
				}
			}
			else
			{
				MyCollectionView.SelectedItem = items[0];
			}
		}
		VisualStateManager.GoToState(MyCollectionView, "Selected");
		CVState.Text = $"State: Selected ({GetSelectedCount()})";
	}

	void OnSetNormal(object sender, EventArgs e)
	{
		if(!MyCollectionView.IsEnabled)
		{
			CVState.Text = "State: Disabled";
			return;
		}
		MyCollectionView.SelectedItem = null;
		if (MyCollectionView.SelectionMode == SelectionMode.Multiple)
		{
			MyCollectionView.SelectedItems?.Clear();
		}
		VisualStateManager.GoToState(MyCollectionView, "Normal");
		CVState.Text = "State: Normal/Unselected";
	}
}