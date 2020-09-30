using System;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.CarouselViewGalleries;

namespace Xamarin.Forms.Controls
{
	public partial class IndicatorsSampleMaximumVisible : ContentPage
	{
		int _maxVisible = 3;
		readonly CarouselItemsGalleryViewModel _vm = new CarouselItemsGalleryViewModel(false, false);

		public IndicatorsSampleMaximumVisible()
		{
			InitializeComponent();
			BindingContext = _vm;
			indicators.MaximumVisible = indicatorsForms.MaximumVisible = _maxVisible;

			UpdateCurrentCount();
		}

		void MaximumVisibleClicked(object sender, EventArgs e)
		{
			_maxVisible--;
			indicators.MaximumVisible = indicatorsForms.MaximumVisible = _maxVisible;
			UpdateCurrentCount();
		}

		void MaximumVisibleAboveItemCountClicked(object sender, EventArgs e)
		{
			_maxVisible = _vm.Items.Count + 1;
			indicators.MaximumVisible = indicatorsForms.MaximumVisible = _maxVisible;
			UpdateCurrentCount();
		}

		void ItemsSourceMinusClicked(object sender, EventArgs e)
		{
			_vm.Items.RemoveAt(0);
			UpdateCurrentCount();
		}

		void ItemsSourcePlusClicked(object sender, EventArgs e)
		{
			_vm.Items.Add(_vm.GetItem(_vm.Items.Count));
			UpdateCurrentCount();
		}

		void UpdateCurrentCount()
		{
			CurrentCount.Text = $"MaximumVisible: {_maxVisible}, item count: {_vm.Items.Count}";
		}
	}
}