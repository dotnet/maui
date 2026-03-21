using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class EmptyViewLoadSimulationPage : ContentPage
	{
		public EmptyViewLoadSimulationPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await Task.Delay(2000);
			collectionView.ItemsSource = (BindingContext as MonkeysViewModel).Monkeys;
		}
	}
}
