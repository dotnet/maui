namespace Maui.Controls.Sample
{
	public partial class VisualTransformOptionsPage : ContentPage
	{
		private VisualTransformViewModel _viewModel;

		public VisualTransformOptionsPage(VisualTransformViewModel viewModel)
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