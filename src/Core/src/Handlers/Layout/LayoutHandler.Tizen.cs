using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public interface IRegisterLayoutUpdate
	{
		void RegisterOnLayoutUpdated();
	}

	public partial class LayoutHandler : ViewHandler<ILayout, Canvas>, IRegisterLayoutUpdate
	{
		bool _layoutUpdatedRegistered;
		Graphics.Rectangle _arrangeCache;

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			VirtualView?.Clip != null ||
			base.NeedsContainer;

		public static void MapBackground(LayoutHandler handler, ILayout layout)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.WrappedNativeView?.UpdateBackground(layout);
		}

		protected override Canvas CreateNativeView()
		{
			if (VirtualView == null)
			{
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Canvas");
			}

			if (NativeParent == null)
			{
				throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");
			}

			var view = new Canvas(NativeParent);

			view.Show();
			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			NativeView.Children.Clear();

			foreach (var child in VirtualView)
			{
				NativeView.Children.Add(child.ToNative(MauiContext));
				if (child.Handler is INativeViewHandler thandler)
				{
					thandler?.SetParent(this);
				}
			}
		}

		public void Add(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			_arrangeCache = default(Graphics.Rectangle);

			NativeView.Children.Add(child.ToNative(MauiContext));
			if (child.Handler is INativeViewHandler thandler)
			{
				thandler?.SetParent(this);
			}
		}

		public void Remove(IView child)
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

			_arrangeCache = default(Graphics.Rectangle);

			if (child.Handler is INativeViewHandler thandler && thandler.NativeView is EvasObject nativeView)
			{
				NativeView.Children.Remove(nativeView);
				thandler.Dispose();
			}
		}

		protected override Graphics.Point ComputeAbsolutePoint(Graphics.Rectangle frame)
		{
			if (_layoutUpdatedRegistered)
				return frame.Location;
			return base.ComputeAbsolutePoint(frame);
		}

		protected override void ConnectHandler(Canvas nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.LayoutUpdated += OnLayoutUpdated;
		}

		protected override void DisconnectHandler(Canvas nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.LayoutUpdated -= OnLayoutUpdated;
		}

		protected void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			if (VirtualView != null && NativeView != null)
			{
				var nativeGeometry = NativeView.Geometry.ToDP();
				if (_arrangeCache == nativeGeometry)
					return;

				_arrangeCache = nativeGeometry;

				if (nativeGeometry.Width > 0 && nativeGeometry.Height > 0)
				{
					VirtualView.InvalidateMeasure();
					VirtualView.InvalidateArrange();

					if (!_layoutUpdatedRegistered)
					{
						nativeGeometry.X = VirtualView.Frame.X;
						nativeGeometry.Y = VirtualView.Frame.Y;
					}

					// revoke margin area, we need to assign area including margin
					nativeGeometry.Width += VirtualView.Margin.HorizontalThickness;
					nativeGeometry.Height += VirtualView.Margin.VerticalThickness;
					nativeGeometry.X -= VirtualView.Margin.Left;
					nativeGeometry.Y -= VirtualView.Margin.Top;

					VirtualView.LayoutManager.Measure(nativeGeometry.Width, nativeGeometry.Height);
					VirtualView.LayoutManager.ArrangeChildren(nativeGeometry);
				}
			}
		}

		public void RegisterOnLayoutUpdated()
		{
			_layoutUpdatedRegistered = true;
		}
	}
}
