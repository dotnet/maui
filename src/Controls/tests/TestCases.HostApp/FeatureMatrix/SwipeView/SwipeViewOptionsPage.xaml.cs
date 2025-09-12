using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class SwipeViewOptionsPage : ContentPage
{
	private SwipeViewViewModel _viewModel;
	private SwipeViewControlMainPage _mainPage;
	private string _selectedContent = "Label";
	private string _selectedSwipeItem = "Label";

	public SwipeViewOptionsPage(SwipeViewViewModel viewModel, SwipeViewControlMainPage mainPage)
	{
		InitializeComponent();
		_viewModel = viewModel;
		_mainPage = mainPage;
		BindingContext = _viewModel;

		_selectedContent = viewModel.SelectedContentType;
		_selectedSwipeItem = viewModel.SelectedSwipeItemType;
		SetInitialRadioSelections();
	}
	public event Action<string, string> SwipeViewOptionsApplied;

	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		_viewModel.SelectedContentType = _selectedContent;
		_viewModel.SelectedSwipeItemType = _selectedSwipeItem;
		SwipeViewOptionsApplied?.Invoke(_selectedContent, _selectedSwipeItem);
		await Navigation.PopAsync();
	}

	private void OnSwipeModeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value || sender is not RadioButton radio)
			return;

		_viewModel.SwipeMode = radio.Content?.ToString() == "Execute"
			? SwipeMode.Execute
			: SwipeMode.Reveal;
	}

	private void OnSwipeBehaviorChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value || sender is not RadioButton radio)
			return;

		_viewModel.SwipeBehaviorOnInvoked = radio.Content?.ToString() switch
		{
			"Close" => SwipeBehaviorOnInvoked.Close,
			"RemainOpen" => SwipeBehaviorOnInvoked.RemainOpen,
			_ => SwipeBehaviorOnInvoked.Auto
		};
	}

	private void OnThresholdTextChanged(object sender, TextChangedEventArgs e)
	{
		if (double.TryParse(e.NewTextValue, out var threshold))
		{
			_viewModel.Threshold = threshold;
		}
	}

	private void OnBackgroundColorClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		_viewModel.BackgroundColor = button.Text switch
		{
			"LightGreen" => Colors.LightGreen,
			"SkyBlue" => Colors.SkyBlue,
			"LightPink" => Colors.LightPink,
			_ => Colors.LightGray
		};
	}

	private void OnSwipeItemBackgroundColorClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		_viewModel.SwipeItemsBackgroundColor = button.Text switch
		{
			"Blue" => Colors.DeepSkyBlue,
			"Pink" => Colors.PaleVioletRed,
			"Yellow" => Color.FromArgb("#FFDB58"),
			_ => Color.FromArgb("#6A5ACD")
		};
	}

	private void OnFlowDirectionChanged(object sender, EventArgs e)
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

	private void OnIsVisibleChanged(object sender, CheckedChangedEventArgs e)
	{
		if (IsVisibleTrue.IsChecked)
			_viewModel.IsVisible = true;
		else if (IsVisibleFalse.IsChecked)
			_viewModel.IsVisible = false;
	}

	private void OnIsEnabledChanged(object sender, CheckedChangedEventArgs e)
	{
		if (IsEnabledTrue.IsChecked)
			_viewModel.IsEnabled = true;
		else if (IsEnabledFalse.IsChecked)
			_viewModel.IsEnabled = false;
	}

	private void OnContentChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value || sender is not RadioButton radio)
			return;

		_selectedContent = radio.Content?.ToString();
	}

	private void OnSwipeItemsChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value || sender is not RadioButton radio)
			return;

		_selectedSwipeItem = radio.Content?.ToString();
	}

	private void SetInitialRadioSelections()
	{
		switch (_selectedContent)
		{
			case "Label":
				TextContent.IsChecked = true;
				break;
			case "Image":
				ImageContent.IsChecked = true;
				break;
			case "CollectionView":
				CollectionContent.IsChecked = true;
				break;
		}

		switch (_selectedSwipeItem)
		{
			case "Label":
				LabelSwipeItem.IsChecked = true;
				break;
			case "IconImageSource":
				IconImageSourceSwipeItem.IsChecked = true;
				break;
			case "Button":
				ButtonSwipeItem.IsChecked = true;
				break;
		}
	}
}