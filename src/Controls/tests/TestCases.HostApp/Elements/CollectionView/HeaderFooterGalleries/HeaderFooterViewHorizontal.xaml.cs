using System.Windows.Input;

namespace Maui.Controls.Sample.CollectionViewGalleries.HeaderFooterGalleries
{
	public partial class HeaderFooterViewHorizontal : ContentPage
	{
		readonly HeaderFooterViewModel _viewModel = new HeaderFooterViewModel(10);

		public HeaderFooterViewHorizontal()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			BindingContext = _viewModel;
		}
	}
}