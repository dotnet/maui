using System;
using static Microsoft.Maui.Controls.Compatibility.Platform.GTK.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Cells
{
	internal class ViewCell : CellBase
	{
		private WeakReference<IVisualElementRenderer> _rendererRef;
		private Gdk.Rectangle _lastAllocation;

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			if (_lastAllocation != allocation)
			{
				_lastAllocation = allocation;

				var viewCell = Cell as Microsoft.Maui.Controls.Compatibility.ViewCell;
				var view = viewCell.View;

				if (view == null)
				{
					return;
				}

				double width = allocation.Width;
				double height = DesiredHeight > 0
								? DesiredHeight
								: Cell.RenderHeight > 0
									? Cell.RenderHeight
									: GetHeightMeasure(viewCell, allocation);

				Layout.LayoutChildIntoBoundingRegionInternal(view, new Rectangle(0, 0, width, height));

				if (_rendererRef == null)
					return;

				IVisualElementRenderer renderer;
				if (_rendererRef.TryGetTarget(out renderer))
				{
					renderer.Container.WidthRequest = (int)width;
				}
			}
		}

		private double GetHeightMeasure(Microsoft.Maui.Controls.Compatibility.ViewCell viewCell, Gdk.Rectangle allocation)
		{
			var request = viewCell.View.Measure(allocation.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

			return request.Request.Height;
		}

		public override void Destroy()
		{
			IVisualElementRenderer renderer;
			if (_rendererRef != null && _rendererRef.TryGetTarget(out renderer) && renderer.Element != null)
			{
				_rendererRef = null;
			}

			base.Destroy();
		}

		protected override void UpdateCell()
		{
			var viewCell = Cell as Microsoft.Maui.Controls.Compatibility.ViewCell;

			if (viewCell != null)
				Device.BeginInvokeOnMainThread(viewCell.SendDisappearing);

			Device.BeginInvokeOnMainThread(viewCell.SendAppearing);

			IVisualElementRenderer renderer;
			if (_rendererRef == null || !_rendererRef.TryGetTarget(out renderer))
				renderer = GetNewRenderer();
			else
			{
				if (renderer.Element != null && renderer == Platform.GetRenderer(renderer.Element))
					renderer.Element.ClearValue(Platform.RendererProperty);

				var type = Internals.Registrar.Registered.GetHandlerType(viewCell.View.GetType());
				if (renderer.GetType() == type || (renderer is DefaultRenderer && type == null))
					renderer.SetElement(viewCell.View);
				else
				{
					renderer = GetNewRenderer();
				}
			}

			Platform.SetRenderer(viewCell.View, renderer);
		}

		private IVisualElementRenderer GetNewRenderer()
		{
			var viewCell = Cell as Microsoft.Maui.Controls.Compatibility.ViewCell;

			if (viewCell.View == null)
			{
				return null;
			}

			var newRenderer = Platform.CreateRenderer(viewCell.View);

			_rendererRef = new WeakReference<IVisualElementRenderer>(newRenderer);
			Add(newRenderer.Container);

			return newRenderer;
		}
	}
}
