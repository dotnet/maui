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
					{
						if (uiView is WebKit.WKWebView webView)
						{
							var existingMaskView = webView.ViewWithTag(InterceptRightClickWebViewMaskView.MaskViewTag);
							existingMaskView?.RemoveFromSuperview();

							// If the view is a WKWebView, we need to intercept right-clicks
							// to show the context menu, so we add a mask view that intercepts
							// right-clicks and passes them to the context menu interaction.
							var maskView = new InterceptRightClickWebViewMaskView(uiView.Bounds);
							webView.AddSubview(maskView);
							maskView.AddInteraction(new MauiUIContextMenuInteraction(handler));
						}
						else
						{
							uiView.AddInteraction(new MauiUIContextMenuInteraction(handler));
						}
					}
				}
				else if (currentInteraction != null)
				{
					uiView.RemoveInteraction(currentInteraction);

					if (uiView is WebKit.WKWebView webView)
					{
						var existingMaskView = webView.ViewWithTag(InterceptRightClickWebViewMaskView.MaskViewTag);
						existingMaskView?.RemoveFromSuperview();
					}
				}
			}
		}

		class InterceptRightClickWebViewMaskView : PlatformView
		{
			public const int MaskViewTag = 9999;
			public InterceptRightClickWebViewMaskView(CGRect frame) : base(frame)
			{
				Tag = MaskViewTag;
				BackgroundColor = UIColor.Clear;
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				UserInteractionEnabled = true;
			}

			public override PlatformView? HitTest(CGPoint point, UIEvent? uievent)
			{
				if (uievent is UIEvent { Type: UIEventType.Touches } touch)
				{
					if (touch.ButtonMask.HasFlag(UIEventButtonMask.Secondary))
						return base.HitTest(point, uievent);
				}

				return null;
			}
		}
#endif

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		internal static void UpdateTransformation(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTransformation(view);
		}
	}
}