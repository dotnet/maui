using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	public class Issue19064 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task FirstItemKeepsItsSizeAfterRecycling()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var collectionView = new CollectionView
			{
				WidthRequest = 360,
				HeightRequest = 240,
				ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
				ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal)
				{
					HorizontalItemSpacing = 8,
					VerticalItemSpacing = 8
				},
				ItemTemplate = new DataTemplate(CreateItemView),
				ItemsSource = CreateItems()
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				var nativeCollectionView = handler.Controller.CollectionView;
				await FlushLayout(nativeCollectionView);

				var initial = GetFirstItemMeasurement(nativeCollectionView);
				var returnedMeasurements = new List<Measurement>();

				for (var pass = 0; pass < 2; pass++)
				{
					nativeCollectionView.ScrollToItem(
						NSIndexPath.FromItemSection(59, 0),
						UICollectionViewScrollPosition.Right,
						false);
					await FlushLayout(nativeCollectionView);

					nativeCollectionView.ScrollToItem(
						NSIndexPath.FromItemSection(0, 0),
						UICollectionViewScrollPosition.Left,
						false);
					await FlushLayout(nativeCollectionView);
					returnedMeasurements.Add(GetFirstItemMeasurement(nativeCollectionView));
				}

				Assert.True(
					returnedMeasurements.All(measurement => measurement.Matches(initial)),
					"The first item's size changed after it was recycled.");
			});
		}

		static Border CreateItemView()
		{
			var content = new Grid
			{
				BackgroundColor = Colors.CornflowerBlue
			};
			content.SetBinding(VisualElement.WidthRequestProperty, nameof(GalleryItem.ItemWidth));
			content.SetBinding(VisualElement.HeightRequestProperty, nameof(GalleryItem.ItemHeight));

			var label = new Label
			{
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};
			label.SetBinding(Label.TextProperty, nameof(GalleryItem.DisplayText));
			content.Add(label);

			var border = new Border
			{
				Padding = 0,
				StrokeThickness = 2,
				Content = content
			};
			border.SetBinding(VisualElement.WidthRequestProperty, nameof(GalleryItem.ItemWidth));
			border.SetBinding(VisualElement.HeightRequestProperty, nameof(GalleryItem.ItemHeight));
			return border;
		}

		static IReadOnlyList<GalleryItem> CreateItems()
		{
			var items = new List<GalleryItem>();
			for (var index = 0; index < 60; index++)
			{
				var sizeGroup = index % 3;
				items.Add(new GalleryItem(
					index,
					100 + (sizeGroup * 100),
					50 + (sizeGroup * 50),
					$"Item {index}"));
			}

			return items;
		}

		static async Task FlushLayout(UICollectionView collectionView)
		{
			collectionView.LayoutIfNeeded();
			await collectionView.PerformBatchUpdatesAsync(() => { });
			collectionView.LayoutIfNeeded();
		}

		static Measurement GetFirstItemMeasurement(UICollectionView collectionView)
		{
			var cell = collectionView.CellForItem(NSIndexPath.FromItemSection(0, 0));
			Assert.NotNull(cell);

			var border = FindSubviews<MauiView>(cell)
				.Select(view => view.View)
				.OfType<Border>()
				.FirstOrDefault();
			Assert.NotNull(border);

			var content = Assert.IsType<Grid>(border.Content);
			return new Measurement(border.Width, border.Height, content.Width, content.Height);
		}

		static IEnumerable<T> FindSubviews<T>(UIView view) where T : UIView
		{
			foreach (var subview in view.Subviews)
			{
				if (subview is T match)
					yield return match;

				foreach (var descendant in FindSubviews<T>(subview))
					yield return descendant;
			}
		}

		sealed record GalleryItem(int Index, double ItemWidth, double ItemHeight, string DisplayText);

		readonly record struct Measurement(
			double ItemWidth,
			double ItemHeight,
			double ContentWidth,
			double ContentHeight)
		{
			const double Tolerance = 0.5;

			public bool Matches(Measurement other) =>
				Math.Abs(ItemWidth - other.ItemWidth) <= Tolerance
				&& Math.Abs(ItemHeight - other.ItemHeight) <= Tolerance
				&& Math.Abs(ContentWidth - other.ContentWidth) <= Tolerance
				&& Math.Abs(ContentHeight - other.ContentHeight) <= Tolerance;
		}
	}
}
