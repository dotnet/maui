using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ItemsViewTests : BaseTestFixture
	{
		public ItemsViewTests()
		{

			DeviceDisplay.SetCurrent(new MockDeviceDisplay());
		}

		[Fact]
		public void VerticalListMeasurement()
		{
			var itemsView = new StructuredItemsView();

			var sizeRequest = itemsView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			Assert.Equal(sizeRequest.Request.Height, scaled.Height);
			Assert.Equal(sizeRequest.Request.Width, scaled.Width);
		}

		[Fact]
		public void HorizontalListMeasurement()
		{
			var itemsView = new StructuredItemsView();

			itemsView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

			var sizeRequest = itemsView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			Assert.Equal(sizeRequest.Request.Height, scaled.Height);
			Assert.Equal(sizeRequest.Request.Width, scaled.Width);
		}

		[Fact]
		public void BindingContextPropagatesLayouts()
		{
			var bindingContext = new object();
			var itemsView = new StructuredItemsView();
			itemsView.BindingContext = bindingContext;
			var linearItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
			itemsView.ItemsLayout = linearItemsLayout;

			// BindingContext is set when ItemsLayout is set
			Assert.Equal(itemsView.BindingContext, linearItemsLayout.BindingContext);

			// BindingContext is updated when BindingContext on ItemsView is changed
			bindingContext = new object();
			itemsView.BindingContext = bindingContext;
			Assert.Equal(itemsView.BindingContext, linearItemsLayout.BindingContext);
		}

		[Fact]
		public async Task SelectedItemsDoesNotLeakCollectionView()
		{
			// Assigning a long-lived INotifyCollectionChanged collection to SelectedItems is the
			// idiomatic multi-selection binding (e.g. a view-model ObservableCollection reused
			// across navigations). SelectableItemsView wraps the value in a SelectionList that
			// subscribes to the collection's CollectionChanged; if that subscription is strong and
			// never torn down, the shared collection roots the whole control. See
			// https://github.com/dotnet/maui/issues/36350
			var sharedSelection = new ObservableCollection<object> { "a", "b", "c" };

			WeakReference weakCollectionView;

			{
				var collectionView = new CollectionView
				{
					SelectionMode = SelectionMode.Multiple,
					SelectedItems = sharedSelection,
				};

				weakCollectionView = new WeakReference(collectionView);
			}

			Assert.False(await weakCollectionView.WaitForCollect(), "CollectionView should not be alive!");
			GC.KeepAlive(sharedSelection);
		}
	}
}