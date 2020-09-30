using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DataTemplateSelectorGallery : ContentPage
	{
		DemoFilteredItemSource _demoFilteredItemSource;

		public DataTemplateSelectorGallery()
		{
			InitializeComponent();

			_demoFilteredItemSource = new DemoFilteredItemSource(filter: ItemMatches);

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.SearchCommand = new Command(() =>
			{
				_demoFilteredItemSource.FilterItems(SearchBar.Text);
				CollectionView.EmptyView = SearchBar.Text;
			});
		}

		private bool ItemMatches(string filter, CollectionViewGalleryTestItem item)
		{
			if (String.IsNullOrEmpty(filter))
			{
				return true;
			}

			return item.Date.DayOfWeek.ToString().ToLower().Contains(filter.ToLower());
		}
	}

	public class WeekendSelector : DataTemplateSelector
	{
		public DataTemplate FridayTemplate { get; set; }
		public DataTemplate DefaultTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var dow = ((CollectionViewGalleryTestItem)item).Date.DayOfWeek;

			return dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday
				? FridayTemplate
				: DefaultTemplate;
		}
	}

	public class SearchTermSelector : DataTemplateSelector
	{
		public DataTemplate DefaultTemplate { get; set; }
		public DataTemplate SymbolsTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var search = ((string)item);

			return search.Any(c => !char.IsLetter(c))
				? SymbolsTemplate
				: DefaultTemplate;
		}
	}
}