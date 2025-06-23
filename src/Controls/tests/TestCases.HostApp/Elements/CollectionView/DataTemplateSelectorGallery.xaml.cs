namespace Maui.Controls.Sample.CollectionViewGalleries
{
	public partial class DataTemplateSelectorGallery : ContentPage
	{
		DemoFilteredItemSource _demoFilteredItemSource;

		public DataTemplateSelectorGallery()
		{
			InitializeComponent();

			_demoFilteredItemSource = new DemoFilteredItemSource(count: 200, filter: ItemMatches);

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

			return item.Date.DayOfWeek.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);
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
				? FridayTemplate!
				: DefaultTemplate!;
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
				? SymbolsTemplate!
				: DefaultTemplate!;
		}
	}
}