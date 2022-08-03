using System;
using ElmSharp;
using PlatformView = ElmSharp.EvasObject;

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
			handler.ToPlatform()?.UpdateTransformation(view);
		}

		protected virtual void OnPlatformViewDeleted()
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

		partial void ConnectingHandler(PlatformView? platformView)
		{
			if (platformView == null)
				return;


			platformView.Deleted += OnPlatformViewDeleted;

			if (platformView is Widget widget)
			{
				widget.Focused += OnFocused;
				widget.Unfocused += OnUnfocused;
			}
		}

		partial void DisconnectingHandler(PlatformView platformView)
		{
			if (platformView == null)
				return;

			platformView.Deleted -= OnPlatformViewDeleted;
		}

		public virtual bool NeedsContainer
		{
			get
			{
				if (VirtualView is IBorderView border)
					return border?.Shape != null || border?.Stroke != null;

				return false;
			}
		}

		void OnPlatformViewDeleted(object? sender, EventArgs e)
		{
			OnPlatformViewDeleted();
		}
	}
}
