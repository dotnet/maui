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

		// Preset button handlers
		private void Preset45Rotate_Clicked(object sender, EventArgs e)
		{
			_viewModel.Rotation = 45.0;
		}

		private void Preset2xScale_Clicked(object sender, EventArgs e)
		{
			_viewModel.Scale = 2.0;
		}

		private void PresetFlipX_Clicked(object sender, EventArgs e)
		{
			_viewModel.ScaleX = -1.0;
			_viewModel.ScaleY = 1.0;
		}

		private void PresetSkew_Clicked(object sender, EventArgs e)
		{
			_viewModel.ScaleX = 1.2;
			_viewModel.ScaleY = 0.8;
			_viewModel.Rotation = 15.0;
		}

		private void Preset3DCube_Clicked(object sender, EventArgs e)
		{
			_viewModel.RotationX = 25.0;
			_viewModel.RotationY = 35.0;
			_viewModel.Rotation = 10.0;
			_viewModel.Scale = 1.2;
		}
	}
}