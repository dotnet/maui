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

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
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