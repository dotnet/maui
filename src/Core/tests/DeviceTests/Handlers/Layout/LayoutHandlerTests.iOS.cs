using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		LayoutView GetNativeLayout(LayoutHandler layoutHandler)
		{
			return layoutHandler.PlatformView;
		}

		double GetNativeChildCount(LayoutHandler layoutHandler)
		{
			return layoutHandler.PlatformView.Subviews.Length;
		}

		double GetNativeChildCount(object platformView)
		{
			return (platformView as UIView).Subviews.Length;
		}

		IReadOnlyList<UIView> GetNativeChildren(LayoutHandler layoutHandler)
		{
			return layoutHandler.PlatformView.Subviews;
		}

		string GetNativeText(UIView view)
		{
			return (view as UILabel).Text;
		}

		async Task AssertZIndexOrder(IReadOnlyList<UIView> children)
		{
			// Lots of ways we could compare the two lists, but dumping them both to comma-separated strings
			// makes it easy to give the test useful output

			string expected = await InvokeOnMainThreadAsync(() =>
			{
				return children.OrderBy(platformView => GetNativeText(platformView))
					.Aggregate("", (str, platformView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(platformView));
			});

			string actual = await InvokeOnMainThreadAsync(() =>
			{
				return children.Aggregate("", (str, platformView) => str + (str.Length > 0 ? ", " : "") + GetNativeText(platformView));
			});

			Assert.Equal(expected, actual);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToImmediateChildren()
		{
			var layout = new LayoutStub();
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			layout.Add(label);
			label.Parent = layout;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				layout.FlowDirection = FlowDirection.RightToLeft;
				layoutHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToDescendants()
		{
			var layout0 = new LayoutStub();
			var layout1 = new LayoutStub() { FlowDirection = FlowDirection.MatchParent };
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			layout0.Add(layout1);
			layout1.Add(label);
			label.Parent = layout1;
			layout1.Parent = layout0;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layout1Handler = CreateHandler<LayoutHandler>(layout1);
				var layout0Handler = CreateHandler<LayoutHandler>(layout0);

				layout0.FlowDirection = FlowDirection.RightToLeft;
				layout0Handler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task FlowDirectionPropagatesToAddedChildren()
		{
			var layout = new LayoutStub() { FlowDirection = FlowDirection.RightToLeft };
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.MatchParent };
			label.Parent = layout;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				layout.Add(label);

				var args = new Maui.Handlers.LayoutHandlerUpdate(0, label);
				layoutHandler.Invoke(nameof(ILayoutHandler.Add), args);

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, labelFlowDirection);
		}

		[Fact, Category(TestCategory.FlowDirection)]
		public async Task DoesNotPropagateToChildrenWithExplicitFlowDirection()
		{
			var layout = new LayoutStub();
			var label = new LabelStub { Text = "Test", FlowDirection = FlowDirection.LeftToRight };
			layout.Add(label);
			label.Parent = layout;

			var labelFlowDirection = await InvokeOnMainThreadAsync(() =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				var layoutHandler = CreateHandler<LayoutHandler>(layout);

				layout.FlowDirection = FlowDirection.RightToLeft;
				layoutHandler.UpdateValue(nameof(IView.FlowDirection));

				return labelHandler.PlatformView.EffectiveUserInterfaceLayoutDirection;
			});

			Assert.Equal(UIUserInterfaceLayoutDirection.LeftToRight, labelFlowDirection);
		}

		[Theory, Category(TestCategory.FlowDirection)]
		[InlineData(0, 84, 0)] // Side status bar on the right edge (iPhone Duo)
		[InlineData(84, 0, 0)]
		[InlineData(0, 0, 40)] // No safe area: the layout's own Padding mirrors as before
		[InlineData(0, 84, 40)]
		[InlineData(0, 84, 0, true)] // CollectionView cell: no native safe-area adjustments, insets come from the cell override
		public async Task RtlChildrenMirrorWithinTheSafeArea(double safeLeft, double safeRight, double paddingLeft, bool cellOverride = false)
		{
			var layout = new VerticalStackLayoutStub { Padding = new Thickness(paddingLeft, 0, 0, 0) };
			var label = new LabelStub { Text = "Test" };
			layout.Add(label);

			var frame = await InvokeOnMainThreadAsync(async () =>
			{
				var labelView = CreateHandler<LabelHandler>(label).PlatformView;
				var parent = new SafeAreaLayoutView
				{
					Insets = cellOverride ? UIEdgeInsets.Zero : new UIEdgeInsets(0, (nfloat)safeLeft, 0, (nfloat)safeRight),
					CellSafeAreaOverride = cellOverride ? new SafeAreaPadding(safeLeft, safeRight, 0, 0) : SafeAreaPadding.Empty,
					Frame = new CGRect(0, 0, 400, 200),
					SemanticContentAttribute = UISemanticContentAttribute.ForceRightToLeft,
					View = layout,
					CrossPlatformLayout = layout,
				};
				parent.AddSubview(labelView);

				return await parent.AttachAndRun(() =>
				{
					parent.SetNeedsLayout();
					parent.LayoutIfNeeded();
					return labelView.Frame;
				});
			});

			// Safe area insets stay on their physical edge; Padding.Left becomes the trailing (right) gap
			Assert.Equal(safeLeft, (double)frame.Left, 1);
			Assert.Equal(400 - safeRight - paddingLeft, (double)frame.Right, 1);
		}

		class SafeAreaLayoutView : LayoutView
		{
			public UIEdgeInsets Insets { get; set; }
			public override UIEdgeInsets SafeAreaInsets => Insets;
		}
	}
}