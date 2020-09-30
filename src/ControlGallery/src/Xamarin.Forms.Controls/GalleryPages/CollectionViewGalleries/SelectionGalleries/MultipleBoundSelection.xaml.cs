using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MultipleBoundSelection : ContentPage
	{
		BoundSelectionModel _vm;

		public MultipleBoundSelection()
		{
			_vm = new BoundSelectionModel();
			BindingContext = _vm;
			InitializeComponent();
		}

		private void ClearAndAdd(object sender, EventArgs e)
		{
			_vm.SelectedItems.Clear();
			_vm.SelectedItems.Add(_vm.Items[1]);
			_vm.SelectedItems.Add(_vm.Items[2]);
		}

		private void ResetClicked(object sender, EventArgs e)
		{
			_vm.SelectedItems = new ObservableCollection<object>
			{
				_vm.Items[1],
				_vm.Items[2]
			};
		}

		private void DirectUpdateClicked(object sender, EventArgs e)
		{
			CollectionView.SelectedItems.Clear();
			CollectionView.SelectedItems.Add(_vm.Items[0]);
			CollectionView.SelectedItems.Add(_vm.Items[3]);
		}
	}
}