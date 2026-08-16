#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	public class Issue19064 : ControlsHandlerTestBase
	{
		const string IssueNumber = "19064";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task FirstItemContentSizeRemainsStableAfterRecycling()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<Border, BorderHandler>();
				});
			});

			var realizedItems = new List<Border>();
			var items = Enumerable.Range(0, 100)
				.Select(index => new GalleryItem
				{
					Index = index,
					Width = index < 3 ? 100 : 300,
					Height = index < 3 ? 50 : 150
				})
				.ToList();

			var collectionView = new CollectionView
			{
				ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
				ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal),
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() =>
				{
					var border = new Border();
					border.SetBinding(VisualElement.WidthRequestProperty, nameof(GalleryItem.Width));
					border.SetBinding(VisualElement.HeightRequestProperty, nameof(GalleryItem.Height));
					realizedItems.Add(border);
					return border;
				})
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				bool IsRealized(int index) =>
					realizedItems.Any(item =>
						item.BindingContext is GalleryItem galleryItem &&
						galleryItem.Index == index &&
						item.IsLoaded &&
						item.Width > 0 &&
						item.Height > 0);

				await AssertEventually(
					() => IsRealized(0),
					timeout: 5000,
					message: "The first item was not initially arranged.");

				var initialItem = realizedItems.First(item =>
					item.BindingContext is GalleryItem { Index: 0 } &&
					item.IsLoaded);
				var initialWidth = initialItem.Width;
				var initialHeight = initialItem.Height;

				collectionView.ScrollTo(50, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(
					() => handler.Controller.CollectionView.IndexPathsForVisibleItems.All(indexPath => indexPath.Item != 0),
					timeout: 5000,
					message: "The first item did not leave the native viewport.");
				await AssertEventually(
					() => IsRealized(50),
					timeout: 5000,
					message: "The distant item was not realized for cell recycling.");

				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(
					() => handler.Controller.CollectionView.IndexPathsForVisibleItems.Any(indexPath => indexPath.Item == 0) &&
						IsRealized(0),
					timeout: 5000,
					message: "The first item was not arranged after returning.");

				var returnedItem = realizedItems.First(item =>
					item.BindingContext is GalleryItem { Index: 0 } &&
					item.IsLoaded);

				Assert.True(
					Math.Abs(initialWidth - 100) < 0.5 &&
					Math.Abs(initialHeight - 50) < 0.5 &&
					Math.Abs(returnedItem.Width - 100) < 0.5 &&
					Math.Abs(returnedItem.Height - 50) < 0.5,
					"The first item's content size must remain 100x50 after recycling.");
			});
		}

		sealed class GalleryItem
		{
			public int Index { get; set; }
			public double Width { get; set; }
			public double Height { get; set; }
		}
	}
}
