﻿#nullable enable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		static ViewHandler()
		{
			FocusManager.GotFocus += FocusManager_GotFocus;
			FocusManager.LostFocus += FocusManager_LostFocus;
		}

		partial void ConnectingHandler(PlatformView? platformView)
		{
			platformView?.SetMauiHandler(this);
		}

		partial void DisconnectingHandler(PlatformView platformView)
		{
			platformView.SetMauiHandler(null);
			UpdateIsFocused(false);
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			// Both Clip and Shadow depend on the Control size.
			handler.ToPlatform().UpdateClip(view);
			handler.ToPlatform().UpdateShadow(view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		public static void MapToolbar(IViewHandler handler, IView view)
		{
			if (view is IToolbarElement tb)
			{
				MapToolbar(handler, tb);
			}
		}

		internal static void MapToolbar(IElementHandler handler, IToolbarElement toolbarElement)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(handler.MauiContext)} null");

			if (toolbarElement.Toolbar != null)
			{
				var toolBar = toolbarElement.Toolbar.ToPlatform(handler.MauiContext);
				handler.MauiContext.GetNavigationRootManager().SetToolbar(toolBar);
			}
		}

		public static void MapContextFlyout(IViewHandler handler, IView view)
		{
			if (view is IContextFlyoutElement contextFlyoutContainer)
			{
				MapContextFlyout(handler, contextFlyoutContainer);
			}
		}

		internal static void MapContextFlyout(IElementHandler handler, IContextFlyoutElement contextFlyoutContainer)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"The handler's {nameof(handler.MauiContext)} cannot be null.");

			if (handler.PlatformView is Microsoft.UI.Xaml.UIElement uiElement)
			{
				if (contextFlyoutContainer.ContextFlyout != null)
				{
					var contextFlyoutHandler = contextFlyoutContainer.ContextFlyout.ToHandler(handler.MauiContext);
					var contextFlyoutPlatformView = contextFlyoutHandler.PlatformView;

					if (contextFlyoutPlatformView is FlyoutBase flyoutBase)
					{
						uiElement.ContextFlyout = flyoutBase;
					}
				}
				else
				{
					uiElement.ClearValue(UIElement.ContextFlyoutProperty);
				}
			}
		}

		static void FocusManager_GotFocus(object? sender, FocusManagerGotFocusEventArgs e)
		{
			if (e.NewFocusedElement is PlatformView platformView && platformView.GetMauiHandler() is { } handler)
			{
				handler.UpdateIsFocused(true);
			}
		}

		static void FocusManager_LostFocus(object? sender, FocusManagerLostFocusEventArgs e)
		{
			if (e.OldFocusedElement is PlatformView platformView && platformView.GetMauiHandler() is { } handler)
			{
				handler.UpdateIsFocused(false);
			}
		}

		void UpdateIsFocused(bool isFocused)
		{
			if (VirtualView is not { } virtualView)
			{
				return;
			}

			bool updateIsFocused = (isFocused && !virtualView.IsFocused) || (!isFocused && virtualView.IsFocused);

			if (updateIsFocused)
			{
				virtualView.IsFocused = isFocused;
			}
		}
	}
}