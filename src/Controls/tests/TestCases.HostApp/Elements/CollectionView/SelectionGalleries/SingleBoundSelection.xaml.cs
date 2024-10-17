namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
{
	public partial class SingleBoundSelection : ContentPage
	{
		readonly BoundSelectionModel _vm;

		public SingleBoundSelection()
		{
			InitializeComponent();
			_vm = new BoundSelectionModel();
			BindingContext = _vm;
		}

		private void ResetClicked(object sender, EventArgs e)
		{
			_vm.SelectedItem = _vm.Items![0];
		}

		private void ClearClicked(object sender, EventArgs e)
		{
			_vm.SelectedItem = null;
		}
	}
}