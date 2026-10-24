#nullable enable
using System;
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
using ContentView = Microsoft.Maui.Controls.ContentView;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.CollectionView)]
public class CollectionViewArrangeLifecycleTests : ControlsHandlerTestBase
{
	[Fact]
	public Task SameSizeNativeLayoutDoesNotRepeatInlineArrange() =>
		WithCell((cell, content) =>
		{
			cell.Bounds = new CGRect(0, 0, 200, 40);
			using var attributes = CreateAttributes(200, 40);
			cell.PreferredLayoutAttributesFittingAttributes(attributes);
			var inlineCount = content.ArrangeCount;
			Assert.True(inlineCount > 0);

			cell.LayoutSubviews();

			Assert.Equal(inlineCount, content.ArrangeCount);
		});

	[Fact]
	public Task SameSizeContentInvalidationStillArrangesInline() =>
		WithCell((cell, content) =>
		{
			cell.Bounds = new CGRect(0, 0, 200, 40);
			using var attributes = CreateAttributes(200, 40);
			cell.PreferredLayoutAttributesFittingAttributes(attributes);
			cell.LayoutSubviews();
			var initialCount = content.ArrangeCount;

			((IPlatformMeasureInvalidationController)cell).InvalidateMeasure(false);
			cell.PreferredLayoutAttributesFittingAttributes(attributes);
			Assert.Equal(initialCount + 1, content.ArrangeCount);

			cell.LayoutSubviews();
			Assert.Equal(initialCount + 1, content.ArrangeCount);
		});

	[Fact]
	public Task ArrangeInducedSameSizeInvalidationDoesNotRepeatArrange() =>
		WithCell((cell, content) =>
		{
			cell.Bounds = new CGRect(0, 0, 200, 40);
			using var attributes = CreateAttributes(200, 40);
			content.OnArrange = () => ((IPlatformMeasureInvalidationController)cell).InvalidateMeasure(false);
			cell.PreferredLayoutAttributesFittingAttributes(attributes);
			cell.LayoutSubviews();
			var initialCount = content.ArrangeCount;

			cell.PreferredLayoutAttributesFittingAttributes(attributes);
			cell.LayoutSubviews();

			Assert.Equal(initialCount, content.ArrangeCount);
		});

	[Fact]
	public Task ChangedConstraintsArrangeOnlyAfterFinalNativeBounds() =>
		WithCell((cell, content) =>
		{
			cell.Bounds = new CGRect(0, 0, 200, 40);
			using var attributes = CreateAttributes(200, 40);
			cell.PreferredLayoutAttributesFittingAttributes(attributes);
			cell.LayoutSubviews();
			var initialCount = content.ArrangeCount;

			using var rotatedAttributes = CreateAttributes(240, 40);
			cell.PreferredLayoutAttributesFittingAttributes(rotatedAttributes);
			Assert.Equal(initialCount, content.ArrangeCount);

			cell.Bounds = new CGRect(0, 0, 240, 40);
			cell.LayoutSubviews();
			Assert.Equal(initialCount + 1, content.ArrangeCount);
			Assert.Equal(240, content.LastBounds.Width);
		});

	[Fact]
	public Task FirstNativeBoundsDifferentFromPreferredAreArranged() =>
		WithCell((cell, content) =>
		{
			cell.Bounds = new CGRect(0, 0, 200, 40);
			using var attributes = CreateAttributes(200, 40);
			cell.PreferredLayoutAttributesFittingAttributes(attributes);
			var inlineCount = content.ArrangeCount;

			cell.Bounds = new CGRect(0, 0, 200, 60);
			cell.LayoutSubviews();

			Assert.Equal(inlineCount + 1, content.ArrangeCount);
			Assert.Equal(60, content.LastBounds.Height);
		});

	async Task WithCell(Action<TemplatedCell2, CountingContentView> assertions)
	{
		EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<CollectionView, CollectionViewHandler2>();
			handlers.AddHandler<CountingContentView, ContentViewHandler>();
		}));

		var content = new CountingContentView();
		var collectionView = new CollectionView { ItemTemplate = new DataTemplate(() => content) };
		await CreateHandlerAsync<CollectionViewHandler2>(collectionView);
		await InvokeOnMainThreadAsync(() =>
		{
			using var cell = new VerticalCell2(CGRect.Empty);
			cell.Bind(collectionView.ItemTemplate, "Item", collectionView);
			assertions(cell, content);
		});
	}

	static UICollectionViewLayoutAttributes CreateAttributes(double width, double height)
	{
		var attributes = UICollectionViewLayoutAttributes.CreateForCell(NSIndexPath.FromRowSection(0, 0));
		attributes.Frame = new CGRect(0, 0, width, height);
		return attributes;
	}

	class CountingContentView : ContentView
	{
		public int ArrangeCount { get; private set; }
		public Rect LastBounds { get; private set; }
		public Action? OnArrange { get; set; }

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint) =>
			new(double.IsPositiveInfinity(widthConstraint) ? 200 : widthConstraint, 40);

		protected override Size ArrangeOverride(Rect bounds)
		{
			ArrangeCount++;
			LastBounds = bounds;
			OnArrange?.Invoke();
			return bounds.Size;
		}
	}
}
