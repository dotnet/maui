using System;
using System.Reflection;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.ContentView
{
	[Category(TestCategory.ContentView)]
	public partial class ContentViewTests
	{
		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToContent()
		{
			var contentView = new ContentViewStub();
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			contentView.PresentedContent = label;

			// Have to set this manually with the stubs, and the propagation code relies on Parentage
			label.Parent = contentView;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.FlowDirection = FlowDirection.RightToLeft;
				contentViewHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToDescendants()
		{
			var contentView = new ContentViewStub();
			var layout1 = new LayoutStub() { FlowDirection = FlowDirection.MatchParent };
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			contentView.PresentedContent = layout1;
			layout1.Add(label);
			layout1.Parent = contentView;
			label.Parent = layout1;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layout1Handler = CreateHandler<LayoutHandler>(layout1);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.FlowDirection = FlowDirection.RightToLeft;
				contentViewHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToUpdatedContent()
		{
			var contentView = new ContentViewStub() { FlowDirection = FlowDirection.RightToLeft };
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			var label2 = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			contentView.PresentedContent = label;
			label.Parent = contentView;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var labelHandler2 = CreateHandler<LabelHandler>(label2);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.PresentedContent = label2;
				label.Parent = null;
				label2.Parent = contentView;
				contentViewHandler.UpdateValue(nameof(IContentView.Content));

				return labelHandler2.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task DoesNotPropagateToContentWithExplicitFlowDirection()
		{
			var contentView = new ContentViewStub();
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.LeftToRight };
			contentView.PresentedContent = label;
			label.Parent = contentView;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				contentView.FlowDirection = FlowDirection.RightToLeft;
				contentViewHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.LeftToRight, labelFlowDirection);
		}

		[Fact]
		public async Task RemoveContentMaskDoesNotThrowWhenDisposed()
		{
			// Verify that removing a subview with an active clip mask does not throw
			// ObjectDisposedException when the underlying CAShapeLayer is already disposed.
			// Related: https://github.com/dotnet/macios/issues/10562
			await InvokeOnMainThreadAsync(() =>
			{
				var contentView = new Microsoft.Maui.Platform.ContentView();
				contentView.Frame = new CGRect(0, 0, 200, 200);

				var content = new UIView { Tag = Microsoft.Maui.Platform.ContentView.ContentTag };
				content.Frame = new CGRect(0, 0, 200, 200);
				contentView.AddSubview(content);

				// Set a clip to trigger _contentMask creation via UpdateClip
				contentView.Clip = new BorderStrokeStub();
				contentView.LayoutSubviews();

				// Verify the mask was created
				Assert.IsAssignableFrom<CAShapeLayer>(content.Layer.Mask);

				// Create a deterministically-disposed CAShapeLayer.
				// A freshly-created layer with zero native retains is guaranteed
				// to have Handle == IntPtr.Zero after Dispose(), regardless of
				// platform-specific retain-count or GC timing behavior.
				var disposedLayer = new CAShapeLayer();
				disposedLayer.Dispose();
				Assert.True(disposedLayer.Handle == IntPtr.Zero, "Disposed layer must have a zeroed Handle");

				// Use reflection to inject the disposed layer into the private
				// _contentMask field, simulating the race condition where iOS
				// deallocates the native layer during view teardown while our
				// managed field still holds a reference.
				var field = typeof(Microsoft.Maui.Platform.ContentView)
					.GetField("_contentMask", BindingFlags.NonPublic | BindingFlags.Instance);
				Assert.NotNull(field);
				field!.SetValue(contentView, disposedLayer);

				// RemoveFromSuperview triggers WillRemoveSubview → RemoveContentMask.
				// Without the Handle guard, this would throw ObjectDisposedException
				// when calling RemoveFromSuperLayer() on the disposed mask.
				var ex = Record.Exception(() => content.RemoveFromSuperview());
				Assert.Null(ex);
			});
		}

		/// <summary>
		/// Minimal IBorderStroke stub for testing clip mask creation.
		/// </summary>
		class BorderStrokeStub : IBorderStroke
		{
			public IShape Shape { get; set; } = new RectangleShape();
			public Paint Stroke { get; set; }
			public double StrokeThickness { get; set; } = 1;
			public LineCap StrokeLineCap { get; set; }
			public LineJoin StrokeLineJoin { get; set; }
			public float[] StrokeDashPattern { get; set; }
			public float StrokeDashOffset { get; set; }
			public float StrokeMiterLimit { get; set; }
		}

		class RectangleShape : IShape
		{
			public PathF PathForBounds(Rect bounds)
			{
				var path = new PathF();
				path.AppendRectangle(bounds);
				return path;
			}
		}
	}
}
