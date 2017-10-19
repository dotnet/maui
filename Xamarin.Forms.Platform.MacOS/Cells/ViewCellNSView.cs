using System;
using AppKit;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class ViewCellNSView : CellNSView
	{
		public ViewCellNSView() : base(NSTableViewCellStyle.Empty)
		{

		}

		WeakReference<IVisualElementRenderer> _rendererRef;

		ViewCell _viewCell;

		public override Element Element => ViewCell;

		public ViewCell ViewCell
		{
			get { return _viewCell; }
			set
			{
				if (_viewCell == value)
					return;
				UpdateCell(value);
			}
		}

		public override void Layout()
		{
			LayoutSubviews();
			base.Layout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				IVisualElementRenderer renderer;
				if (_rendererRef != null && _rendererRef.TryGetTarget(out renderer) && renderer.Element != null)
				{
					Platform.DisposeModelAndChildrenRenderers(renderer.Element);

					_rendererRef = null;
				}
			}

			base.Dispose(disposing);
		}

		void LayoutSubviews()
		{
			var contentFrame = Frame;
			var view = ViewCell.View;

			Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(view, contentFrame.ToRectangle());

			if (_rendererRef == null)
				return;

			IVisualElementRenderer renderer;
			if (_rendererRef.TryGetTarget(out renderer))
				renderer.NativeView.Frame = view.Bounds.ToRectangleF();
		}

		IVisualElementRenderer GetNewRenderer()
		{
			var newRenderer = Platform.CreateRenderer(_viewCell.View);
			_rendererRef = new WeakReference<IVisualElementRenderer>(newRenderer);
			AddSubview(newRenderer.NativeView);
			return newRenderer;
		}

		void UpdateCell(ViewCell cell)
		{
			if (_viewCell != null)
				Device.BeginInvokeOnMainThread(_viewCell.SendDisappearing);

			_viewCell = cell;

			Device.BeginInvokeOnMainThread(_viewCell.SendAppearing);

			IVisualElementRenderer renderer;
			if (_rendererRef == null || !_rendererRef.TryGetTarget(out renderer))
				renderer = GetNewRenderer();
			else
			{
				if (renderer.Element != null && renderer == Platform.GetRenderer(renderer.Element))
					renderer.Element.ClearValue(Platform.RendererProperty);

				var type = Internals.Registrar.Registered.GetHandlerTypeForObject(_viewCell.View);
				var reflectableType = renderer as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : renderer.GetType();
				if (rendererType == type || (renderer is DefaultRenderer && type == null))
					renderer.SetElement(_viewCell.View);
				else
				{
					//when cells are getting reused the element could be already set to another cell
					//so we should dispose based on the renderer and not the renderer.Element
					Platform.DisposeRendererAndChildren(renderer);
					renderer = GetNewRenderer();
				}
			}

			Platform.SetRenderer(_viewCell.View, renderer);
		}
	}
}