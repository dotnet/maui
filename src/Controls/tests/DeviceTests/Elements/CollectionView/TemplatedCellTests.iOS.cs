using System.Threading.Tasks;
using CoreGraphics;
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
	public class TemplatedCellTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(FlowDirection.LeftToRight)]
		[InlineData(FlowDirection.RightToLeft)]
		public async Task ContentRemainsAlignedAfterNativeBoundsChange(FlowDirection flowDirection)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			await InvokeOnMainThreadAsync(() =>
			{
				var collectionView = new CollectionView { FlowDirection = flowDirection };
				var handler = CreateHandler<CollectionViewHandler2>(collectionView);
				var content = new CellArrangeCountingGrid { FlowDirection = flowDirection };
				content.Add(new Label { Text = "Item", HeightRequest = 40 });

				using var cell = new TemplatedCell2(new CGRect(0, 0, 100, 40))
				{
					ScrollDirection = UICollectionViewScrollDirection.Vertical,
					SemanticContentAttribute = flowDirection == FlowDirection.RightToLeft
						? UISemanticContentAttribute.ForceRightToLeft
						: UISemanticContentAttribute.ForceLeftToRight
				};
				cell.Bind(new DataTemplate(() => content), null, collectionView);

				void ResizeCell(double width, int expectedFinalArrangeCount = 1)
				{
					using var indexPath = NSIndexPath.FromItemSection(0, 0);
					using var attributes = UICollectionViewLayoutAttributes.CreateForCell(indexPath);
					attributes.Frame = new CGRect(0, 0, width, 40);
					var preferredAttributes = cell.PreferredLayoutAttributesFittingAttributes(attributes);
					var arrangeCount = content.ArrangeCount;

					// UIKit applies the preferred bounds after the managed sizing pass.
					cell.Bounds = new CGRect(CGPoint.Empty, preferredAttributes.Size);
					cell.ContentView.Bounds = cell.Bounds;
					cell.LayoutSubviews();

					var nativeContent = content.ToPlatform();
					Assert.Equal(0, (double)nativeContent.Frame.X, 1);
					Assert.Equal(width, (double)nativeContent.Frame.Width, 1);
					Assert.Equal(width, content.Width, 1);

					Assert.Equal(arrangeCount + expectedFinalArrangeCount, content.ArrangeCount);
					cell.LayoutSubviews();
					Assert.Equal(arrangeCount + expectedFinalArrangeCount, content.ArrangeCount);
				}

				try
				{
					ResizeCell(300, expectedFinalArrangeCount: 0);
					ResizeCell(600);
					ResizeCell(300);
					cell.PrepareForReuse();
					cell.Bind(new DataTemplate(() => content), null, collectionView);
					ResizeCell(300, expectedFinalArrangeCount: 0);
					ResizeCell(600);
				}
				finally
				{
					cell.Unbind();
					((IElementHandler)handler).DisconnectHandler();
				}
			});
		}

		sealed class CellArrangeCountingGrid : Grid
		{
			public int ArrangeCount { get; private set; }

			protected override Size ArrangeOverride(Rect bounds)
			{
				ArrangeCount++;
				return base.ArrangeOverride(bounds);
			}
		}
	}
}
