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
			CollectionView.SelectionChangedCommand = new Command(UpdateSelectionInfoCommand);

			UpdateSelectionInfo(Enumerable.Empty<CollectionViewGalleryTestItem>(), Enumerable.Empty<CollectionViewGalleryTestItem>());
			UpdateSelectionInfoCommand();
		}

		void CollectionViewOnSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			UpdateSelectionInfo(args.CurrentSelection, args.PreviousSelection);
		}

		void UpdateSelectionInfo(IEnumerable<object> currentSelectedItems, IEnumerable<object> previousSelectedItems)
		{
			var previous = ToList(previousSelectedItems);
			var current = ToList(currentSelectedItems);

			if (string.IsNullOrEmpty(previous))
			{
				previous = "[none]";
			}

			if (string.IsNullOrEmpty(current))
			{
				current = "[none]";
			}

			SelectedItemsEvent.Text = $"Selection (event): {current}";
			PreviousItemsEvent.Text = $"Previous (event): {previous}";
		}

		void UpdateSelectionInfoCommand()
		{
			var current = "[none]";

			if (CollectionView.SelectedItem != null)
			{
				current = ((CollectionViewGalleryTestItem)CollectionView?.SelectedItem)?.Caption;
			}

			SelectedItemsCommand.Text = $"Selection (command): {current}";
		}

		static string ToList(IEnumerable<object> items)
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