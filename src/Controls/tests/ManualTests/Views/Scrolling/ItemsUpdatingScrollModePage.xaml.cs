using Microsoft.Maui.ManualTests.Controls;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class ItemsUpdatingScrollModePage : ContentPage
	{
		public ItemsUpdatingScrollModePage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModelWithDelay();
		}

		void OnItemsUpdatingScrollModeChanged(object sender, EventArgs e)
		{
			collectionView.ItemsUpdatingScrollMode = (ItemsUpdatingScrollMode)(sender as EnumPicker).SelectedItem;
		}
	}
}
