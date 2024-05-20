using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.HeaderFooterGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HeaderFooterView : ContentPage
	{
		readonly HeaderFooterViewModel _viewModel = new HeaderFooterViewModel(0);

		public HeaderFooterView()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.BindingContext = _viewModel;
		}
	}

	internal class HeaderFooterViewModel : DemoFilteredItemSource
	{
		public HeaderFooterViewModel(int count = 50, Func<string, CollectionViewGalleryTestItem, bool>? filter = null) : base(count, filter)
		{
		}

		public ICommand AddCommand => new Command(async () => await AddItemsAsync());

		public ICommand ClearCommand => new Command(() => Items.Clear());

		public string HeaderText => "This Is A Header";

		public string FooterText => "This Is A Footer";

		async Task AddItemsAsync()
		{
			await Task.Delay(TimeSpan.FromSeconds(1));
			AddItems(Items, 2);
		}
	}
}