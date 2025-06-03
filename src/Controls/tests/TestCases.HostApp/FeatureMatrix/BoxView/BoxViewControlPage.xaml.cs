using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class BoxViewControlPage : NavigationPage
	{
		private BoxViewViewModal _viewModel;

		public BoxViewControlPage()
		{
			_viewModel = new BoxViewViewModal();
			PushAsync(new BoxViewControlMainPage(_viewModel));
		}
	}

	public partial class BoxViewControlMainPage : ContentPage
	{
		private BoxViewViewModal _viewModel;

		public BoxViewControlMainPage(BoxViewViewModal viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private void OnColorRadioButtonChanged(object sender, EventArgs e)
		{
			if (sender is RadioButton radioButton && radioButton.IsChecked)
			{
				switch (radioButton.Value.ToString())
				{
					case "Red":
						_viewModel.Color = Colors.Red;
						break;
					case "Blue":
						_viewModel.Color = Colors.Blue;
						break;
					default:
						_viewModel.Color = Colors.Transparent;
						break;
				}
			}
		}

		private void OnCornerRadiusEntryChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.NewTextValue))
				return;

			var parts = e.NewTextValue.Split(',');

			if (parts.Length != 4)
				return;

			if (float.TryParse(parts[0].Trim(), out float topLeft) &&
				float.TryParse(parts[1].Trim(), out float topRight) &&
				float.TryParse(parts[2].Trim(), out float bottomLeft) &&
				float.TryParse(parts[3].Trim(), out float bottomRight))
			{
				_viewModel.CornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight);
			}
		}

		private void OnResetChangesClicked(object sender, EventArgs e)
		{
			_viewModel.ResetToDefaults();
		}

		private void OnOpacityChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(e.NewTextValue, out double value))
			{
				if (value >= 0 && value <= 1)
					_viewModel.Opacity = value;
			}
		}
	}
}