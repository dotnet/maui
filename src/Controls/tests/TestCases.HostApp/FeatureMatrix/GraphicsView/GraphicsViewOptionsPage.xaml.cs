using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class GraphicsViewOptionsPage : ContentPage
	{
		private GraphicsViewViewModel _viewModel;

		public GraphicsViewOptionsPage(GraphicsViewViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private void ApplyButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}

		// Add event handlers for GraphicsView options
		private void OnDrawableChanged(object sender, EventArgs e)
		{
			if (DrawablePicker.SelectedItem != null)
			{
				_viewModel.DrawableType = DrawablePicker.SelectedItem.ToString();
			}
		}

		private void BackgroundColorButton_Clicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			if (button != null)
			{
				switch (button.Text)
				{
					case "Gray":
						_viewModel.BackgroundColor = Colors.Gray;
						break;
					case "Light Blue":
						_viewModel.BackgroundColor = Colors.LightBlue;
						break;
					default:
						_viewModel.BackgroundColor = Colors.Transparent;
						break;
				}
			}
		}

		private void OnFlowDirectionChanged(object sender, EventArgs e)
		{
			_viewModel.FlowDirection = FlowDirectionLTR.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}

		private void OnIsEnabledCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			_viewModel.IsEnabled = IsEnabledTrueRadio.IsChecked;
		}

		private void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			_viewModel.IsVisible = IsVisibleTrueRadio.IsChecked;
		}
	}
}
