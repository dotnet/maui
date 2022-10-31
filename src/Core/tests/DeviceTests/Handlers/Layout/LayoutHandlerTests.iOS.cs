using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.Layout
{
	public partial class LayoutHandlerTests
	{
		[Fact(DisplayName = "Shadow Initializes Correctly")]
		public async Task ShadowInitializesCorrectly()
		{
			var xPlatShadow = new ShadowStub
			{
				Offset = new Point(10, 10),
				Opacity = 1.0f,
				Radius = 2.0f
			};

			var layout = new LayoutStub
			{
				Height = 50,
				Width = 50
			};

			layout.Shadow = xPlatShadow;

			await ValidateHasColor(layout, Colors.Red, () => xPlatShadow.Paint = new SolidPaint(Colors.Red));
		}

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

		Task ValidateHasColor(ILayout layout, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeLayout = GetNativeLayout(CreateHandler(layout));
				action?.Invoke();
				nativeLayout.AssertContainsColorAsync(color);
			});
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
	}
}