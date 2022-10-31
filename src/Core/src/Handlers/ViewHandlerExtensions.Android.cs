using System;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using static Android.Views.View;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui
{
	internal static partial class ViewHandlerExtensions
	{
		// TODO: Possibly reconcile this code with LayoutViewGroup.OnLayout
		// If you make changes here please review if those changes should also
		// apply to LayoutViewGroup.OnLayout
		internal static Size LayoutVirtualView(
			this IPlatformViewHandler viewHandler,
			int l, int t, int r, int b,
			Func<Rect, Size>? arrangeFunc = null)
		{
			var context = viewHandler.MauiContext?.Context;
			var virtualView = viewHandler.VirtualView;
			var platformView = viewHandler.PlatformView;

			if (context == null || virtualView == null || platformView == null)
			{
				return Size.Zero;
			}

			var destination = context.ToCrossPlatformRectInReferenceFrame(l, t, r, b);
			arrangeFunc ??= virtualView.Arrange;
			return arrangeFunc(destination);
		}

		// TODO: Possibly reconcile this code with LayoutViewGroup.OnMeasure
		// If you make changes here please review if those changes should also
		// apply to LayoutViewGroup.OnMeasure
		internal static Size MeasureVirtualView(
			this IPlatformViewHandler viewHandler,
			int platformWidthConstraint,
			int platformHeightConstraint,
			Func<double, double, Size>? measureFunc = null)
		{
			var context = viewHandler.MauiContext?.Context;
			var virtualView = viewHandler.VirtualView;
			var platformView = viewHandler.PlatformView;

			if (context == null || virtualView == null || platformView == null)
			{
				return Size.Zero;
			}

			var deviceIndependentWidth = platformWidthConstraint.ToDouble(context);
			var deviceIndependentHeight = platformHeightConstraint.ToDouble(context);

			var widthMode = MeasureSpec.GetMode(platformWidthConstraint);
			var heightMode = MeasureSpec.GetMode(platformHeightConstraint);

			measureFunc ??= virtualView.Measure;
			var measure = measureFunc(deviceIndependentWidth, deviceIndependentHeight);

			// If the measure spec was exact, we should return the explicit size value, even if the content
			// measure came out to a different size
			var width = widthMode == MeasureSpecMode.Exactly ? deviceIndependentWidth : measure.Width;
			var height = heightMode == MeasureSpecMode.Exactly ? deviceIndependentHeight : measure.Height;

			var platformWidth = context.ToPixels(width);
			var platformHeight = context.ToPixels(height);

			// Minimum values win over everything
			platformWidth = Math.Max(platformView.MinimumWidth, platformWidth);
			platformHeight = Math.Max(platformView.MinimumHeight, platformHeight);

			return new Size(platformWidth, platformHeight);
		}

		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var Context = viewHandler.MauiContext?.Context;
			var platformView = viewHandler.ToPlatform();
			var virtualView = viewHandler.VirtualView;

			if (platformView == null || virtualView == null || Context == null)
			{
				return Size.Zero;
			}

			// Create a spec to handle the native measure
			var widthSpec = Context.CreateMeasureSpec(widthConstraint, virtualView.Width, virtualView.MaximumWidth);
			var heightSpec = Context.CreateMeasureSpec(heightConstraint, virtualView.Height, virtualView.MaximumHeight);

			var packed = PlatformInterop.MeasureAndGetWidthAndHeight(platformView, widthSpec, heightSpec);
			var measuredWidth = (int)(packed >> 32);
			var measuredHeight = (int)(packed & 0xffffffffL);

			// Convert back to xplat sizes for the return value
			return Context.FromPixels(measuredWidth, measuredHeight);
		}

		internal static void PlatformArrangeHandler(this IViewHandler viewHandler, Rect frame)
		{
			var platformView = viewHandler.ToPlatform();

			var Context = viewHandler.MauiContext?.Context;
			var MauiContext = viewHandler.MauiContext;

			if (platformView == null || MauiContext == null || Context == null)
			{
				return;
			}

			if (frame.Width < 0 || frame.Height < 0)
			{
				// This is a legacy layout value from Controls, nothing is actually laying out yet so we just ignore it
				return;
			}

			var left = Context.ToPixels(frame.Left);
			var top = Context.ToPixels(frame.Top);
			var bottom = Context.ToPixels(frame.Bottom);
			var right = Context.ToPixels(frame.Right);

			var viewParent = platformView.Parent;
			if (viewParent?.LayoutDirection == LayoutDirection.Rtl && viewParent is View parentView)
			{
				// Determine the flipped left/right edges for the RTL layout
				var width = right - left;
				left = parentView.Width - left - width;
				right = left + width;
			}

			platformView.Layout((int)left, (int)top, (int)right, (int)bottom);

			viewHandler.Invoke(nameof(IView.Frame), frame);
		}

		internal static void SetupContainerFromHandler(this IPlatformViewHandler viewHandler, Func<IPlatformViewHandler, View> createContainerView)
		{
			var context = viewHandler.MauiContext?.Context;
			var platformView = viewHandler.PlatformView;
			var containerView = viewHandler.ContainerView;

			if (context == null || platformView == null || containerView != null)
				return;

			var oldParent = (ViewGroup?)platformView.Parent;
			var oldIndex = oldParent?.IndexOfChild(platformView);
			oldParent?.RemoveView(platformView);

			containerView = viewHandler.ContainerView ?? createContainerView.Invoke(viewHandler);
			((ViewGroup)containerView).AddView(platformView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(containerView, idx);
			else
				oldParent?.AddView(containerView);
		}

		internal static void RemoveContainerFromHandler(this IPlatformViewHandler viewHandler, Action<IPlatformViewHandler> clearContainerView)
		{
			var context = viewHandler.MauiContext?.Context;
			var platformView = viewHandler.PlatformView;
			var containerView = viewHandler.ContainerView;

			if (context == null || platformView == null || containerView == null || platformView.Parent != containerView)
				return;

			var oldParent = (ViewGroup?)containerView.Parent;

			var oldIndex = oldParent?.IndexOfChild(containerView);
			oldParent?.RemoveView(containerView);

			((ViewGroup)containerView).RemoveAllViews();
			clearContainerView.Invoke(viewHandler);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(platformView, idx);
			else
				oldParent?.AddView(platformView);
		}
	}
}
