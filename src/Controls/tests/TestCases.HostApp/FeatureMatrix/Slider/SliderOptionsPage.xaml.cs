using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample
{

	public partial class SliderOptionsPage : ContentPage
	{
		private SliderViewModel _viewModel;

		public SliderOptionsPage(SliderViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private void ApplyButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}

		private void OnMinimumChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(MinimumEntry.Text, out double min))
			{
				_viewModel.Minimum = min;
			}
		}

		private void OnMaximumChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(MaximumEntry.Text, out double max))
			{
				_viewModel.Maximum = max;
			}
		}

		private void OnValueChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(ValueEntry.Text, out double value))
			{
				_viewModel.Value = value;
			}
		}
		private void MinTrackColorButton_Clicked(object sender, EventArgs e)
		{
			var button = (Button)sender;
			if (button.Text == "Green")
				_viewModel.MinTrackColor = Colors.Green;
			else if (button.Text == "Yellow")
				_viewModel.MinTrackColor = Colors.Yellow;
		}

		private void MaxTrackColorButton_Clicked(object sender, EventArgs e)
		{
			var button = (Button)sender;
			if (button.Text == "Red")
				_viewModel.MaxTrackColor = Colors.Red;
			else if (button.Text == "Orange")
				_viewModel.MaxTrackColor = Colors.Orange;
		}

		private void BackgroundColorButton_Clicked(object sender, EventArgs e)
		{
			var button = (Button)sender;
			if (button.Text == "Gray")
				_viewModel.BackgroundColor = Colors.Gray;
			else if (button.Text == "Light Blue")
				_viewModel.BackgroundColor = Colors.LightBlue;
		}
		private void ThumbColorButton_Clicked(object sender, EventArgs e)
		{
			var button = (Button)sender;
			if (button.Text == "Red")
				_viewModel.ThumbColor = Colors.Red;
			else if (button.Text == "Green")
				_viewModel.ThumbColor = Colors.Green;
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

		private void ThumbImageSourceButton_Clicked(object sender, EventArgs e)
		{
			_viewModel.ThumbImageSource = "coffee.png";
		}
	}
}
