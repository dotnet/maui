using Microsoft.Maui.ManualTests.Controls;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSnapPointsPage : ContentPage
	{
		public VerticalListSnapPointsPage()
		{
			InitializeComponent();
			BindingContext = new MonkeysViewModel();
		}

		void OnSnapPointsTypeSelectedIndexChanged(object sender, EventArgs e)
		{
			ItemsLayout itemsLayout = (ItemsLayout)collectionView.ItemsLayout;
			itemsLayout.SnapPointsType = (SnapPointsType)(sender as EnumPicker).SelectedItem;
		}

		void OnSnapPointsAlignmentSelectedIndexChanged(object sender, EventArgs e)
		{
			ItemsLayout itemsLayout = (ItemsLayout)collectionView.ItemsLayout;
			itemsLayout.SnapPointsAlignment = (SnapPointsAlignment)(sender as EnumPicker).SelectedItem;
		}
	}
}
