using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
{
	public partial class MultipleBoundSelection : ContentPage
	{
		readonly BoundSelectionModel _vm;

		public MultipleBoundSelection()
		{
			_vm = new BoundSelectionModel();
			BindingContext = _vm;
			InitializeComponent();
		}

		private void ClearAndAdd(object sender, EventArgs e)
		{
			_vm.SelectedItems!.Clear();
			_vm.SelectedItems!.Add(_vm.Items![1]);
			_vm.SelectedItems!.Add(_vm.Items![2]);
		}

		private void ResetClicked(object sender, EventArgs e)
		{
			_vm.SelectedItems = new ObservableCollection<object>
			{
				_vm.Items![1],
				_vm.Items![2]
			};
		}

		private void DirectUpdateClicked(object sender, EventArgs e)
		{
			TestCollectionView.SelectedItems.Clear();
			TestCollectionView.SelectedItems.Add(_vm.Items![0]);
			TestCollectionView.SelectedItems.Add(_vm.Items![3]);
		}
	}
}