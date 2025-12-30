using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class GridOptionsPage : ContentPage
{
	private GridViewModel _viewModel;

	public GridOptionsPage(GridViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnPaddingChanged(object sender, TextChangedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(PaddingEntry?.Text))
			return;

		try
		{
			string[] parts = PaddingEntry.Text.Split(',');
			if (parts.Length == 4 &&
				double.TryParse(parts[0], out double left) &&
				double.TryParse(parts[1], out double top) &&
				double.TryParse(parts[2], out double right) &&
				double.TryParse(parts[3], out double bottom))
			{
				_viewModel.Padding = new Thickness(left, top, right, bottom);
			}
		}
		catch { }

	}

	private void OnBackgroundColorChanged(object sender, EventArgs e)
	{
		if (BindingContext is GridViewModel vm && sender is Button button && button.Text is string color)
		{
			switch (color)
			{
				case "Red":
					vm.BackgroundColor = Colors.Red;
					break;
				case "Gray":
					vm.BackgroundColor = Colors.Gray;
					break;
				default:
					vm.BackgroundColor = Colors.White;
					break;
			}
		}
	}

	private void OnHorizontalOptionClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		switch (button.Text)
		{
			case "Fill":
				_viewModel.HorizontalOptions = LayoutOptions.Fill;
				break;
			case "Start":
				_viewModel.HorizontalOptions = LayoutOptions.Start;
				break;
			case "Center":
				_viewModel.HorizontalOptions = LayoutOptions.Center;
				break;
			case "End":
				_viewModel.HorizontalOptions = LayoutOptions.End;
				break;
		}
	}

	private void OnVerticalOptionClicked(object sender, EventArgs e)
	{
		if (sender is not Button button)
			return;

		switch (button.Text)
		{
			case "Fill":
				_viewModel.VerticalOptions = LayoutOptions.Fill;
				break;
			case "Start":
				_viewModel.VerticalOptions = LayoutOptions.Start;
				break;
			case "Center":
				_viewModel.VerticalOptions = LayoutOptions.Center;
				break;
			case "End":
				_viewModel.VerticalOptions = LayoutOptions.End;
				break;
		}
	}

	private void OnRowTypeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		if (BindingContext is GridViewModel vm && sender is RadioButton radio)
		{
			vm.RowDefinitionType = radio.Content?.ToString() ?? "Star";
		}
	}

	private void OnColumnTypeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!e.Value)
			return;

		if (BindingContext is GridViewModel vm && sender is RadioButton radio)
		{
			vm.ColumnDefinitionType = radio.Content?.ToString() ?? "Star";
		}
	}

	private void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (BindingContext is GridViewModel vm)
		{
			vm.IsVisible = e.Value;
		}
	}

	private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (BindingContext is GridViewModel vm)
		{
			vm.FlowDirection = e.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
		}
	}

	private void OnNestedGridChanged(object sender, CheckedChangedEventArgs e)
	{
		if (BindingContext is GridViewModel vm)
		{
			vm.ShowNestedGrid = e.Value;
		}
	}
}