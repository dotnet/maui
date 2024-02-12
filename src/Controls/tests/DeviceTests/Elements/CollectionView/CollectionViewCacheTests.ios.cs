using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
#if !MACCATALYST
	public partial class CollectionViewTests
	{
		public class CacheTestCollectionView : CollectionView { }

		class CacheTestCollectionViewHandler : ReorderableItemsViewHandler<CacheTestCollectionView>
		{
			protected override ItemsViewController<CacheTestCollectionView> CreateController(CacheTestCollectionView itemsView, ItemsViewLayout layout)
			{
				return new CacheTestItemsViewController(itemsView, layout);
			}
		}

		class CacheTestItemsViewController : ItemsViewController<CacheTestCollectionView>
		{
			protected override bool IsHorizontal { get; }

			public UICollectionViewDelegateFlowLayout DelegateFlowLayout { get; private set; }

			public CacheTestItemsViewController(CacheTestCollectionView reorderableItemsView, ItemsViewLayout layout) : base(reorderableItemsView, layout)
			{
			}

			protected override UICollectionViewDelegateFlowLayout CreateDelegator()
			{
				DelegateFlowLayout = new CacheMissCountingDelegate(ItemsViewLayout, this);
				return DelegateFlowLayout;
			}
		}

		internal class CacheMissCountingDelegate : ItemsViewDelegator<CacheTestCollectionView, ItemsViewController<CacheTestCollectionView>>
		{
			bool _trackCacheMisses;
			public int CacheMissCount { get; set; }

			public CacheMissCountingDelegate(ItemsViewLayout itemsViewLayout, ItemsViewController<CacheTestCollectionView> itemsViewController) : base(itemsViewLayout, itemsViewController)
			{
			}

			public override void WillDisplayCell(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
			{
				_trackCacheMisses = true;
			}

			public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
			{
				var itemSize = base.GetSizeForItem(collectionView, layout, indexPath);

				if (ViewController.Layout is UICollectionViewFlowLayout flowLayout)
				{
					// This is totally a cheat from a unit-testing perspective; we know how the item size cache
					// functions internally (that it will return the estimated size if the item size is not found in the cache),
					// and we're relying on that for this test. Normally we wouldn't do this, but we need to ensure that further
					// code changes don't break the cache again, and the only observable outside consequence is that the scrolling
					// for the CollectionView becomes "janky". We don't want to rely entirely on human perception of "jankiness" to 
					// catch this problem. 

					// If the size comes back as EstimatedItemSize, we'll know that the actual value was not found in the cache.

					if (_trackCacheMisses && itemSize == flowLayout.EstimatedItemSize)
					{
						CacheMissCount += 1;
					}
				}

				return itemSize;
			}
		}

		internal class ItemModel
		{
			public int Index { get; set; }
			public double HeightRequest => 40 * (Index + 1);
		}

		[Fact]
		public async Task EnsureCellSizesAreCached()
		{
			SetupBuilder();

			var collectionView = new CacheTestCollectionView()
			{
				// Deliberately choosing a height which will cause rounding issues which can caused pathological 
				// sizing/layout loops if handled wrong 
				HeightRequest = 654.66666
			};

			var template = new DataTemplate(() =>
			{
				var content = new Label() { Text = "Howdy" };
				content.SetBinding(VisualElement.HeightRequestProperty, new Binding(nameof(VisualElement.HeightRequest)));
				return content;
			});

			// Build up a view model that's got enough items to ensure scrolling and template reuse
			int itemCount = 15;
			var source = new List<ItemModel>();
			for (int n = 0; n < itemCount; n++)
			{
				source.Add(new ItemModel() { Index = n });
			}

			collectionView.ItemTemplate = template;
			collectionView.ItemsSource = source;

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CacheTestCollectionViewHandler>(collectionView, async handler =>
			{
				// Wait until the CollectionView is actually rendering
				await WaitForUIUpdate(frame, collectionView);

				// Tell it to scroll to the bottom (and give it time to do so)
				collectionView.ScrollTo(itemCount - 1);
				await Task.Delay(1000);

				// Now back to the top				
				collectionView.ScrollTo(0);
				await Task.Delay(1000);

				if (handler.Controller is CacheTestItemsViewController controller
					&& controller.DelegateFlowLayout is CacheMissCountingDelegate cacheMissCounter)
				{
					// Different screen sizes and timings mean this isn't 100% predictable. But we can work out some conditions
					// which will tell us that the cache is completely broken and test for those.

					// With 15 items in the list, we can assume a minimum of 15 size cache misses until the cache is populated.
					// Plus at least one for the initial proxy item measurement. 

					// The bugs we are trying to avoid clear the cache prematurely and cause _every_ GetSizeForItem call
					// to miss the cache, so scrolling top to bottom and back with 15 items we would see at _least_ 30 
					// cache misses, likely more (since item sizes will be retrieved more than once as items scroll). 

					// If we have fewer than 20 cache misses, we at least know that the cache isn't being wiped as we scroll.
					int missCountThreshold = 20;
					int actualMissCount = cacheMissCounter.CacheMissCount;
					Assert.True(actualMissCount < missCountThreshold, $"Cache miss count {actualMissCount} was higher than the threshold value of {missCountThreshold}");
				}
				else
				{
					// Something went wrong with this test
					Assert.Fail("Wrong controller type in the test; is the handler registration broken?");
				}
			});
		}
	}
#endif
}
