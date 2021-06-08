using System;
using ElmSharp;
using NativeView = ElmSharp.EvasObject;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		static partial void MappingFrame(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationX(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapTranslationY(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScale(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleX(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapScaleY(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotation(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationX(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapRotationY(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorX(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		public static void MapAnchorY(ViewHandler handler, IView view)
		{
			UpdateTransformation(handler, view);
		}

		internal static void UpdateTransformation(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view);
		}

		protected virtual void OnNativeViewDeleted()
		{
		}

		protected virtual void OnFocused()
		{
		}

		protected virtual void OnUnfocused()
		{
		}

		protected void OnFocused(object? sender, EventArgs e)
		{
			OnFocused();
		}

		protected void OnUnfocused(object? sender, EventArgs e)
		{
			OnUnfocused();
		}

		partial void ConnectingHandler(NativeView? nativeView)
		{
			if (nativeView == null)
				return;


			nativeView.Deleted += OnNativeViewDeleted;

			if (nativeView is Widget widget)
			{
				widget.Focused += OnFocused;
				widget.Unfocused += OnUnfocused;
			}
		}

		partial void DisconnectingHandler(NativeView? nativeView)
		{
			if (nativeView == null)
				return;

			nativeView.Deleted -= OnNativeViewDeleted;
		}

		void OnNativeViewDeleted(object? sender, EventArgs e)
		{
			OnNativeViewDeleted();
		}
	}
}
