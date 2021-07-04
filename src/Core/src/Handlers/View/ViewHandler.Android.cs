using System;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using NativeView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		MauiAccessibilityDelegate? AccessibilityDelegate { get; set; }

		partial void DisconnectingHandler(NativeView? nativeView)
		{
			if (nativeView.IsAlive() && AccessibilityDelegate != null)
			{
				AccessibilityDelegate.Handler = null;
				ViewCompat.SetAccessibilityDelegate(nativeView, null);
				AccessibilityDelegate = null;
			}
		}

		static partial void MappingFrame(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorX(view);
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorY(view);
		}

		public static void MapTranslationX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateTranslationX(view);
		}

		public static void MapTranslationY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateTranslationY(view);
		}

		public static void MapScale(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateScale(view);
		}

		public static void MapScaleX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateScaleX(view);
		}

		public static void MapScaleY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateScaleY(view);
		}

		public static void MapRotation(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateRotation(view);
		}

		public static void MapRotationX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateRotationX(view);
		}

		public static void MapRotationY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateRotationY(view);
		}

		public static void MapAnchorX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorX(view);
		}

		public static void MapAnchorY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorY(view);
		}

		static partial void MappingSemantics(ViewHandler handler, IView view)
		{
			if (handler.NativeView == null)
				return;

			if (view.Semantics != null &&
				handler is ViewHandler viewHandler &&
				viewHandler.AccessibilityDelegate == null &&
				ViewCompat.GetAccessibilityDelegate(handler.NativeView as NativeView) == null)
			{
				if (handler.NativeView is not NativeView nativeView)
					return;

				if (nativeView is AndroidX.AppCompat.Widget.SearchView sv)
					nativeView = sv.FindViewById(Resource.Id.search_button)!;

				if (!string.IsNullOrWhiteSpace(view.Semantics.Hint) || !string.IsNullOrWhiteSpace(view.Semantics.Description))
				{
					if (viewHandler.AccessibilityDelegate == null)
					{
						viewHandler.AccessibilityDelegate = new MauiAccessibilityDelegate() { Handler = viewHandler };
						ViewCompat.SetAccessibilityDelegate(nativeView, viewHandler.AccessibilityDelegate);
					}
				}
				else if (viewHandler.AccessibilityDelegate != null)
				{
					viewHandler.AccessibilityDelegate = null;
					ViewCompat.SetAccessibilityDelegate(nativeView, null);
				}

				if (viewHandler.AccessibilityDelegate != null)
				{
					nativeView.ImportantForAccessibility = ImportantForAccessibility.Yes;
				}
			}
		}

		public void OnInitializeAccessibilityNodeInfo(NativeView? host, AccessibilityNodeInfoCompat? info)
		{
			var semantics = VirtualView?.Semantics;

			if (semantics == null)
				return;

			if (info == null)
				return;

			string? newText = null;
			string? newContentDescription = null;

			var desc = semantics.Description;
			if (!string.IsNullOrEmpty(desc))
			{
				// Edit Text fields won't read anything for the content description
				if (host is EditText)
					newText = $"{desc}, {((EditText)host).Text}";
				else
					newContentDescription = desc;
			}

			var hint = semantics.Hint;
			if (!string.IsNullOrEmpty(hint))
			{
				// info HintText won't read anything back when using TalkBack pre API 26
				if (NativeVersion.IsAtLeast(26))
				{
					info.HintText = hint;

					if (host is EditText)
						info.ShowingHintText = false;
				}
				else
				{
					if (host is TextView tv)
					{
						newText = newText ?? tv.Text;
						newText = $"{newText}, {hint}";
					}
					else
					{
						if (newContentDescription != null)
						{
							newText = $"{newContentDescription}, {hint}";
						}
						else
						{
							newText = $"{hint}";
						}
					}

					newContentDescription = null;
				}
			}

			if (!String.IsNullOrWhiteSpace(newContentDescription))
				info.ContentDescription = newContentDescription;

			if (!String.IsNullOrWhiteSpace(newText))
				info.Text = newText;
		}

		class MauiAccessibilityDelegate : AccessibilityDelegateCompat
		{
			public ViewHandler? Handler { get; set; }

			public MauiAccessibilityDelegate()
			{
			}

			public override void OnInitializeAccessibilityNodeInfo(NativeView? host, AccessibilityNodeInfoCompat? info)
			{
				base.OnInitializeAccessibilityNodeInfo(host, info);
				Handler?.OnInitializeAccessibilityNodeInfo(host, info);
			}
		}
	}
}