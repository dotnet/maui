using System;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SingleBoundSelection : ContentPage
	{
		BoundSelectionModel _vm;

		public SingleBoundSelection()
		{
			InitializeComponent();
			_vm = new BoundSelectionModel();
			BindingContext = _vm;
		}

		private void ResetClicked(object sender, EventArgs e)
		{
			_vm.SelectedItem = _vm.Items[0];
		}

		private void ClearClicked(object sender, EventArgs e)
		{
			_vm.SelectedItem = null;
		}
	}
}