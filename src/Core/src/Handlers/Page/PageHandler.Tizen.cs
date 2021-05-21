using System;
using Tizen.UIExtensions.Common;
using NativeView = ElmSharp.EvasObject;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : EViewHandler<IPage, PageView>, INativeViewHandler
	{
		protected override PageView CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Page");
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");

			var view = new PageView(NativeParent)
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange
			};
			view.Show();
			return view;
		}

		protected override void ConnectHandler(PageView nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.LayoutUpdated += OnLayoutUpdated;
		}

		protected override void DisconnectHandler(PageView nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.LayoutUpdated -= OnLayoutUpdated;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.Measure;
			NativeView.CrossPlatformArrange = VirtualView.Arrange;
		}

		public static void MapTitle(PageHandler handler, IPage page)
		{
		}

		public static void MapContent(PageHandler handler, IPage page)
		{
			handler.UpdateContent();
		}

		protected void OnLayoutUpdated(object sender, LayoutEventArgs e)
		{
			if (VirtualView != null && NativeView != null)
			{
				var nativeGeometry = NativeView.Geometry.ToDP();
				if (nativeGeometry.Width > 0 && nativeGeometry.Height > 0)
				{
					VirtualView.InvalidateMeasure();
					VirtualView.InvalidateArrange();
					VirtualView.Measure(nativeGeometry.Width, nativeGeometry.Height);
					VirtualView.Arrange(nativeGeometry);
				}
			}
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.Children.Add(VirtualView.Content.ToNative(MauiContext, false));
			// TODO : Fix me later
			//if (VirtualView.Content.Handler is INativeViewHandler thandler)
			//{
			//	thandler?.SetParent(this);
			//}
		}
	}
}