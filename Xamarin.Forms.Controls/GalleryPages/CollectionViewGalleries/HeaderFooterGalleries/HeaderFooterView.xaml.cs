using System;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.HeaderFooterGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HeaderFooterView : ContentPage
	{
		readonly HeaderFooterViewModel _viewModel = new HeaderFooterViewModel(3);

		public HeaderFooterView()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.BindingContext = _viewModel;
		}
	}

	internal class HeaderFooterViewModel : DemoFilteredItemSource
	{
		public HeaderFooterViewModel(int count = 50, Func<string, CollectionViewGalleryTestItem, bool> filter = null) : base(count, filter)
		{
		}

		public string HeaderText => "This Is A Header";

		public string FooterText => "This Is A Footer";
	}
}