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
			var previous = previousSelectedItems.ToCommaSeparatedList();
			var current = currentSelectedItems.ToCommaSeparatedList();

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

			if (CollectionView.SelectionMode == SelectionMode.Multiple)
			{
				current = CollectionView?.SelectedItems.ToCommaSeparatedList();
			}
			else if (CollectionView.SelectionMode == SelectionMode.Single)
			{
				current = ((CollectionViewGalleryTestItem)CollectionView?.SelectedItem)?.Caption;
			}

			SelectedItemsCommand.Text = $"Selection (command): {current}";
		}
	}
}