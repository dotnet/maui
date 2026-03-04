using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Platform;
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
#if MACCATALYST
			if (view is IContextFlyoutElement contextFlyoutContainer)
			{
				MapContextFlyout(handler, contextFlyoutContainer);
			}
#endif
		}

#if MACCATALYST
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		internal static void MapContextFlyout(IElementHandler handler, IContextFlyoutElement contextFlyoutContainer)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"The handler's {nameof(handler.MauiContext)} cannot be null.");

			if (handler.PlatformView is PlatformView uiView)
			{
				MauiUIContextMenuInteraction? currentInteraction = null;

				foreach (var interaction in uiView.Interactions)
				{
					if (interaction is MauiUIContextMenuInteraction menuInteraction)
						currentInteraction = menuInteraction;
				}

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
#endif

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
			view.InvalidateMeasure();

			// Only propagate to descendants on runtime changes, not initial connect.
			// On initial connect, children aren't in the tree yet and are born with
			// _safeAreaInvalidated = true, so they'll self-evaluate on first layout.
			if (handler.IsConnectingHandler())
				return;

			// When SafeAreaEdges changes on a view, its descendants may need to re-evaluate
			// whether a parent is now handling their safe area edges. UIKit does not fire
			// SafeAreaInsetsDidChange on descendants for MAUI property changes, so we must
			// explicitly walk the tree to clear the _parentHandlesSafeArea cache and trigger
			// re-evaluation on the next layout pass.
			InvalidateDescendantSafeAreas(handler.ToPlatform());
		}

		// Walks the native subview tree and marks every descendant MauiView/MauiScrollView as
		// needing safe-area re-validation. This clears the cached _parentHandlesSafeArea value
		// so each descendant re-evaluates whether a parent now handles its safe area edges.
		static void InvalidateDescendantSafeAreas(UIView? view)
		{
			if (view is null)
				return;
			foreach (var subview in view.Subviews)
			{
				if (subview is MauiView mauiView)
					mauiView.InvalidateSafeArea();
				else if (subview is MauiScrollView mauiScrollView)
					mauiScrollView.InvalidateSafeArea();
				InvalidateDescendantSafeAreas(subview);
			}
		}
	}
}