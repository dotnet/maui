using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class BoxViewControlPage : NavigationPage
	{
		private BoxViewViewModel _viewModel;

		public BoxViewControlPage()
		{
			_viewModel = new BoxViewViewModel();
			PushAsync(new BoxViewControlMainPage(_viewModel));
		}
	}

	public partial class BoxViewControlMainPage : ContentPage
	{
		private BoxViewViewModel _viewModel;

		public BoxViewControlMainPage(BoxViewViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private void OnCornerRadiusEntryChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.NewTextValue))
				return;

			var parts = e.NewTextValue.Split(',');

			if (parts.Length == 1)
			{
				if (double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double uniform))
					_viewModel.CornerRadius = new CornerRadius(uniform);
			}
			else if (parts.Length == 4)
			{
				if (double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double topLeft) &&
					double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double topRight) &&
					double.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double bottomLeft) &&
					double.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double bottomRight))
				{
					_viewModel.CornerRadius = new CornerRadius(topLeft, topRight, bottomLeft, bottomRight);
				}
			}
		}

		private void OnResetChangesClicked(object sender, EventArgs e)
		{
			_viewModel.Reset();
		}

		private void OnOpacityChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.NewTextValue))
			{
				_viewModel.Opacity = 1.0;
				return;
			}

			if (double.TryParse(e.NewTextValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && value >= 0 && value <= 1)
				_viewModel.Opacity = value;
		}

		private void OnWidthChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.NewTextValue))
			{
				_viewModel.Width = 200;
				return;
			}

			if (double.TryParse(e.NewTextValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && value >= 0)
				_viewModel.Width = value;
		}

		private void OnHeightChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.NewTextValue))
			{
				_viewModel.Height = 100;
				return;
			}

			if (double.TryParse(e.NewTextValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && value >= 0)
				_viewModel.Height = value;
		}
	}
}
