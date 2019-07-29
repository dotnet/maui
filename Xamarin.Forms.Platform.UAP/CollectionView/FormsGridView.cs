using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

namespace Xamarin.Forms.Platform.UAP
{
	// TODO hartez 2018/06/06 10:01:48 Consider whether this should be internal; it might be that we just want to make the ItemsPanel resources configurable in CollectionViewRenderer
	internal class FormsGridView : GridView
	{
		int _maximumRowsOrColumns;
		ItemsWrapGrid _wrapGrid;

		public FormsGridView()
		{
			// TODO hartez 2018/06/06 09:52:16 Do we need to clean this up? If so, where?	
			RegisterPropertyChangedCallback(ItemsPanelProperty, ItemsPanelChanged);
			Loaded += OnLoaded;
		}

		public int MaximumRowsOrColumns
		{
			get => _maximumRowsOrColumns;
			set
			{
				_maximumRowsOrColumns = value;
				if (_wrapGrid != null)
				{
					_wrapGrid.MaximumRowsOrColumns = MaximumRowsOrColumns;
				}
			}
		}

		// TODO hartez 2018/06/06 10:01:32 Probably should just create a local enum for this?	
		public void UseHorizontalItemsPanel()
		{
			ItemsPanel =
				(ItemsPanelTemplate)Windows.UI.Xaml.Application.Current.Resources["HorizontalGridItemsPanel"];
		}

		public void UseVerticalalItemsPanel()
		{
			ItemsPanel =
				(ItemsPanelTemplate)Windows.UI.Xaml.Application.Current.Resources["VerticalGridItemsPanel"];
		}

		void FindItemsWrapGrid()
		{
			_wrapGrid = this.GetFirstDescendant<ItemsWrapGrid>();

			if (_wrapGrid == null)
			{
				return;
			}

			_wrapGrid.MaximumRowsOrColumns = MaximumRowsOrColumns;
		}

		void ItemsPanelChanged(DependencyObject sender, DependencyProperty dp)
		{
			FindItemsWrapGrid();
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			FindItemsWrapGrid();
		}
	}
}