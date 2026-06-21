using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ManualTests.Controls;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class ScrollToByIndexWithGroupingPage : ContentPage
	{
		public ScrollToByIndexWithGroupingPage()
		{
			InitializeComponent();
			BindingContext = new GroupedAnimalsViewModel();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			// Scroll to third cat - items/groups are indexed from zero.
			collectionView.ScrollTo(2, 1, (ScrollToPosition)enumPicker.SelectedItem, animateSwitch.IsToggled);
		}

		void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			Debug.WriteLine("HorizontalDelta: " + e.HorizontalDelta);
			Debug.WriteLine("VerticalDelta: " + e.VerticalDelta);
			Debug.WriteLine("HorizontalOffset: " + e.HorizontalOffset);
			Debug.WriteLine("VerticalOffset: " + e.VerticalOffset);
			Debug.WriteLine("FirstVisibleItemIndex: " + e.FirstVisibleItemIndex);
			Debug.WriteLine("CenterItemIndex: " + e.CenterItemIndex);
			Debug.WriteLine("LastVisibleItemIndex: " + e.LastVisibleItemIndex);
		}
	}
}
