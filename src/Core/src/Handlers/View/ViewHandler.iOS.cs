using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		public static void MapContextFlyout(IViewHandler handler, IView view)
		{
			// Before context flyouts on iOS, they only worked on Mac Catalyst
			if (!OperatingSystem.IsMacCatalyst() && !RuntimeFeature.AreMobileContextFlyoutsEnabled)
				return;

			if (view is IContextFlyoutElement contextFlyoutContainer)
			{
				// Views without a context flyout never get an interaction, so there is nothing to remove
				if (handler.IsConnectingHandler() && contextFlyoutContainer.ContextFlyout is null)
					return;

				MapContextFlyout(handler, contextFlyoutContainer);
			}
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		internal static void MapContextFlyout(IElementHandler handler, IContextFlyoutElement contextFlyoutContainer)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"The handler's {nameof(handler.MauiContext)} cannot be null.");

			if (handler.PlatformView is PlatformView uiView)
			{
				var currentInteraction = GetContextMenuInteraction(uiView);

				if (contextFlyoutContainer.ContextFlyout != null)
				{
					if (currentInteraction == null)
						uiView.AddInteraction(new MauiUIContextMenuInteraction(handler));
				}
				else if (currentInteraction != null)
				{
					uiView.RemoveInteraction(currentInteraction);
				}
			}
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		static MauiUIContextMenuInteraction? GetContextMenuInteraction(PlatformView uiView)
		{
			foreach (var interaction in uiView.Interactions)
			{
				if (interaction is MauiUIContextMenuInteraction menuInteraction)
					return menuInteraction;
			}

			return null;
		}

		partial void DisconnectingHandler(PlatformView platformView)
		{
			if (VirtualView is not IContextFlyoutElement { ContextFlyout: { } contextFlyout })
				return;

			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				if (GetContextMenuInteraction(platformView) is { } interaction)
					platformView.RemoveInteraction(interaction);
			}

			contextFlyout.Handler?.DisconnectHandler();
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			// During the initial setup, MappingFrame will take care of everything
			if (handler.IsConnectingHandler())
				return;

			UpdateTransformation(handler, view);
		}

		internal static void UpdateTransformation(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}

		internal static void MapSafeAreaEdges(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				return;
			}

			if (handler.PlatformView is PlatformView platformView)
			{
				MauiView.InvalidateSafeArea(platformView);
			}

			view.InvalidateMeasure();
		}
	}
}