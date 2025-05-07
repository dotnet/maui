namespace Maui.Controls.Sample;

public partial class CarouselViewControlsPage : ContentPage
{
	 private CarouselViewViewModel _viewModel;

        public CarouselViewControlsPage()
        {
             InitializeComponent();
            _viewModel = new CarouselViewViewModel();
            BindingContext = _viewModel;
        }

        private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
        {
			BindingContext = _viewModel = new CarouselViewViewModel();
            await Navigation.PushAsync(new CarouselViewOptionsPage(_viewModel));        }

}