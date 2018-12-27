using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SelectionModeGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public SelectionModeGallery()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();
			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			var selectionModeSelector = new EnumSelector<SelectionMode>(() => CollectionView.SelectionMode,
				mode => CollectionView.SelectionMode = mode);

			Grid.Children.Add(selectionModeSelector);

			CollectionView.SelectionChanged += CollectionViewOnSelectionChanged;
			CollectionView.SelectionChangedCommand = new Command(() =>
				SelectedItemsCommand.Text = 
				$"SelectionChangedCommand, selection is: {((CollectionViewGalleryTestItem)CollectionView.SelectedItem).Caption}");
		}

		void CollectionViewOnSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			var previous = ToList(args.PreviousSelection);
			var current = ToList(args.CurrentSelection);
			SelectedItemsEvent.Text = $"Selected (from event): {current}; Was: {previous}";
		}

		static string ToList(IReadOnlyList<object> items)
		{
			if (items == null)
			{
				return string.Empty;
			}

			return items.Aggregate(string.Empty, 
				(s, o) => s + (s.Length == 0 ? "" : ", ") + ((CollectionViewGalleryTestItem)o).Caption);
		}
	}
}