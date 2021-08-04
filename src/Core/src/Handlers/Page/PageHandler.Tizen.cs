using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using NativeView = ElmSharp.EvasObject;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<IView, Page>, INativeViewHandler
	{
		INativeViewHandler? _contentHandler;

		protected override Page CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Page");
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");

			var view = new Page(NativeParent)
			{
				BackgroundColor = EColor.White
			};
			view.Show();
			return view;
		}

		protected override void ConnectHandler(Page nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.LayoutUpdated += OnLayoutUpdated;
		}

		protected override void DisconnectHandler(Page nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.LayoutUpdated -= OnLayoutUpdated;
		}

		public static void MapTitle(PageHandler handler, IView page)
		{
		}

		public static void MapContent(PageHandler handler, IView page)
		{
			handler.UpdateContent();
		}

		protected void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			if (VirtualView != null && NativeView != null)
			{
				var nativeGeometry = NativeView.Geometry.ToDP();
				if (nativeGeometry.Width > 0 && nativeGeometry.Height > 0)
				{
					nativeGeometry.X = 0;
					nativeGeometry.Y = 0;
					VirtualView.InvalidateMeasure();
					VirtualView.InvalidateArrange();
					VirtualView.Measure(nativeGeometry.Width, nativeGeometry.Height);
					VirtualView.Arrange(nativeGeometry);
				}
			}
		}

		public override void NativeArrange(Graphics.Rectangle frame)
		{
			// empty on purpose
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.Children.Clear();
			_contentHandler?.Dispose();
			_contentHandler = null;

			if (VirtualView is IContentView cv && cv.Content is IView view)
			{
				NativeView.Children.Add(view.ToNative(MauiContext));

				if (view.Handler is INativeViewHandler thandler)
				{
					thandler?.SetParent(this);
					_contentHandler = thandler;
				}
			}
		}
	}
}