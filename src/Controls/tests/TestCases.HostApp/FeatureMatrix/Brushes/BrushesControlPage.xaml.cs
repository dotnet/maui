using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class BrushesControlPage : NavigationPage
	{
		public BrushesControlPage()
		{
			var vm = new BrushesViewModel();
			var main = new BrushesControlMainPage(vm);
			PushAsync(main);
		}
	}

	public partial class BrushesControlMainPage : ContentPage
	{
		readonly BrushesViewModel _viewModel;

		public BrushesControlMainPage(BrushesViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		private async void OnOptionsClicked(object sender, EventArgs e)
		{
			_viewModel.BrushOpacity = 1;
			_viewModel.HasBrush.ToString();
			_viewModel.BrushTarget.ToString();

			await Navigation.PushAsync(new BrushesOptionsPage(_viewModel));
		}
	}
}