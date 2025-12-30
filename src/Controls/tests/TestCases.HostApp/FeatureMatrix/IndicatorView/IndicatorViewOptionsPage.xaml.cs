using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class IndicatorViewOptionsPage : ContentPage
{
	private readonly IndicatorViewViewModel _viewModel;

	public IndicatorViewOptionsPage(IndicatorViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private void ApplyButton_Clicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
	}

	private void OnIndicatorColorClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			var selectedColor = GetColorFromName(button.Text);
			if (selectedColor != null)
			{
				_viewModel.IndicatorColor = selectedColor;
			}
		}
	}

	private void OnSelectedIndicatorColorClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			var selectedColor = GetColorFromName(button.Text);
			if (selectedColor != null)
			{
				_viewModel.SelectedIndicatorColor = selectedColor;
			}
		}
	}

	private Color GetColorFromName(string colorName)
	{
		return colorName switch
		{
			"Brown" => Colors.Brown,
			"Blue" => Colors.Blue,
			"Red" => Colors.Red,
			"Green" => Colors.Green,
			"Gray" => Colors.Gray,
			"DarkBlue" => Colors.DarkBlue,
			"Orange" => Colors.Orange,
			"Purple" => Colors.Purple,
			_ => Colors.Black
		};
	}

	private void OnHideSingleChanged(object sender, EventArgs e)
	{
		if (sender is RadioButton radioButton && radioButton.IsChecked)
		{
			_viewModel.HideSingle = radioButton.Content?.ToString() == "True";
		}
	}

	private void OnIndicatorsShapeChanged(object sender, EventArgs e)
	{
		if (sender is RadioButton radioButton && radioButton.IsChecked)
		{
			_viewModel.IndicatorsShape = radioButton.Content?.ToString() switch
			{
				"Circle" => IndicatorShape.Circle,
				"Square" => IndicatorShape.Square,
				_ => _viewModel.IndicatorsShape
			};
		}
	}

	private void OnFlowDirectionChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.FlowDirection = radioButton.Content.ToString() == "LTR"
				? FlowDirection.LeftToRight
				: FlowDirection.RightToLeft;
		}
	}

	private void OnIsEnabledChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.IsEnabled = radioButton.Content.ToString() == "True";
		}
	}

	private void OnIsVisibleChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.IsVisible = radioButton.Content.ToString() == "True";
		}
	}

	private void OnShadowChanged(object sender, CheckedChangedEventArgs e)
	{
		if (sender is RadioButton radioButton && e.Value)
		{
			_viewModel.HasShadow = radioButton.Content.ToString() == "True";
		}
	}

	private void OnIconTemplateClicked(object sender, EventArgs e)
	{
		_viewModel.SetIconTemplate();
	}

	private void OnStarTemplateClicked(object sender, EventArgs e)
	{
		_viewModel.SetStarTemplate();
	}

	private void OnHeartTemplateClicked(object sender, EventArgs e)
	{
		_viewModel.SetHeartTemplate();
	}

}

