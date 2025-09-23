using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class BrushesControlPage : NavigationPage
	{
		private BrushesViewModel _viewModel;
		public BrushesControlPage()
		{
			_viewModel = new BrushesViewModel();
			PushAsync(new BrushesControlMainPage(_viewModel));
		}
	}

	public partial class BrushesControlMainPage : ContentPage
	{
		private BrushesViewModel _viewModel;

		public BrushesControlMainPage(BrushesViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		private void OnCompareClicked(object sender, EventArgs e)
		{
			if (BindingContext is BrushesViewModel vm)
			{
				vm.CompareBrushesCommand?.Execute(null);
			}
		}

		private async void OnOptionsClicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new BrushesViewModel();
			await Navigation.PushAsync(new BrushesOptionsPage(_viewModel));
		}
	}
}