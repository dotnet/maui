using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class EmptyViewSwapPage : ContentPage
	{
		public EmptyViewSwapPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
			ToggleEmptyView(false);
		}

		void OnEmptyViewSwitchToggled(object sender, ToggledEventArgs e)
		{
			ToggleEmptyView((sender as Switch).IsToggled);
		}

		void ToggleEmptyView(bool isToggled)
		{
			collectionView.EmptyView = isToggled ? Resources["BasicEmptyView"] : Resources["AdvancedEmptyView"];
		}
	}
}
