using System;
using ElmSharp;
using NativeView = ElmSharp.EvasObject;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
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
			handler.GetWrappedNativeView()?.UpdateTransformation(view);
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

		partial void DisconnectingHandler(NativeView nativeView)
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
