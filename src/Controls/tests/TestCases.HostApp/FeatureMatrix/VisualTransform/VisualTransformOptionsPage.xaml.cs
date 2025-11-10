namespace Maui.Controls.Sample
{
	public partial class VisualTransformOptionsPage : ContentPage
	{
		private VisualTransformViewModal _viewModel;

		private void OnAnchorXChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(e.NewTextValue, out var value))
			{
				_viewModel.AnchorX = value;
				AnchorXLabel.Text = $"AnchorX: {value:F2}";
			}
		}

		private void OnAnchorYChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(e.NewTextValue, out var value))
			{
				_viewModel.AnchorY = value;
				AnchorYLabel.Text = $"AnchorY: {value:F2}";
			}
		}

		public VisualTransformOptionsPage(VisualTransformViewModal viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private void ApplyButton_Clicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}


	}
}