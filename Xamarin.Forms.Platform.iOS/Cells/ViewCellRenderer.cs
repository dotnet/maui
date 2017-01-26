using System;
using System.ComponentModel;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public class ViewCellRenderer : CellRenderer
	{
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var viewCell = (ViewCell)item;

			var cell = reusableCell as ViewTableCell;
			if (cell == null)
				cell = new ViewTableCell(item.GetType().FullName);
			else
				cell.ViewCell.PropertyChanged -= ViewCellPropertyChanged;

			viewCell.PropertyChanged += ViewCellPropertyChanged;
			cell.ViewCell = viewCell;

			SetRealCell(item, cell);

			WireUpForceUpdateSizeRequested(item, cell, tv);

			UpdateBackground(cell, item);
			UpdateIsEnabled(cell, viewCell);
			return cell;
		}

		static void UpdateIsEnabled(ViewTableCell cell, ViewCell viewCell)
		{
			cell.UserInteractionEnabled = viewCell.IsEnabled;
			cell.TextLabel.Enabled = viewCell.IsEnabled;
		}

		void ViewCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var viewCell = (ViewCell)sender;
			var realCell = (ViewTableCell)GetRealCell(viewCell);

			if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled(realCell, viewCell);
		}

		internal class ViewTableCell : UITableViewCell, INativeElementView
		{
			WeakReference<IVisualElementRenderer> _rendererRef;
			ViewCell _viewCell;

			Element INativeElementView.Element => ViewCell;
			internal bool SupressSeparator { get; set; }
			bool _disposed;

			public ViewTableCell(string key) : base(UITableViewCellStyle.Default, key)
			{
			}

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

			public override void LayoutSubviews()
			{
				//This sets the content views frame.
				base.LayoutSubviews();

				//TODO: Determine how best to hide the separator line when there is an accessory on the cell
				if (SupressSeparator && Accessory == UITableViewCellAccessory.None)
				{
					var oldFrame = Frame;
					ContentView.Bounds = new RectangleF(oldFrame.Location, new SizeF(oldFrame.Width, oldFrame.Height + 0.5f));
				}

				var contentFrame = ContentView.Frame;
				var view = ViewCell.View;

				Layout.LayoutChildIntoBoundingRegion(view, contentFrame.ToRectangle());

				if (_rendererRef == null)
					return;

				IVisualElementRenderer renderer;
				if (_rendererRef.TryGetTarget(out renderer))
					renderer.NativeView.Frame = view.Bounds.ToRectangleF();
			}

			public override SizeF SizeThatFits(SizeF size)
			{
				IVisualElementRenderer renderer;
				if (!_rendererRef.TryGetTarget(out renderer))
					return base.SizeThatFits(size);

				if (renderer.Element == null)
					return SizeF.Empty;

				double width = size.Width;
				var height = size.Height > 0 ? size.Height : double.PositiveInfinity;
				var result = renderer.Element.Measure(width, height);

				// make sure to add in the separator if needed
				var finalheight = (float)result.Request.Height + (SupressSeparator ? 0f : 1f) / UIScreen.MainScreen.Scale;
				return new SizeF(size.Width, finalheight);
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing)
				{
					IVisualElementRenderer renderer;
					if (_rendererRef != null && _rendererRef.TryGetTarget(out renderer) && renderer.Element != null)
					{
						var platform = renderer.Element.Platform as Platform;
						if (platform != null)
							platform.DisposeModelAndChildrenRenderers(renderer.Element);

						_rendererRef = null;
					}

					_viewCell = null;
				}

				_disposed = true;

				base.Dispose(disposing);
			}

			IVisualElementRenderer GetNewRenderer()
			{
				var newRenderer = Platform.CreateRenderer(_viewCell.View);
				_rendererRef = new WeakReference<IVisualElementRenderer>(newRenderer);
				ContentView.AddSubview(newRenderer.NativeView);
				return newRenderer;
			}

			void UpdateCell(ViewCell cell)
			{
				ICellController cellController = _viewCell;
				if (cellController != null)
					Device.BeginInvokeOnMainThread(cellController.SendDisappearing);

				_viewCell = cell;
				cellController = cell;

				Device.BeginInvokeOnMainThread(cellController.SendAppearing);

				IVisualElementRenderer renderer;
				if (_rendererRef == null || !_rendererRef.TryGetTarget(out renderer))
					renderer = GetNewRenderer();
				else
				{
					if (renderer.Element != null && renderer == Platform.GetRenderer(renderer.Element))
						renderer.Element.ClearValue(Platform.RendererProperty);

					var type = Registrar.Registered.GetHandlerType(_viewCell.View.GetType());
					if (renderer.GetType() == type || (renderer is Platform.DefaultRenderer && type == null))
						renderer.SetElement(_viewCell.View);
					else
					{
						//when cells are getting reused the element could be already set to another cell
						//so we should dispose based on the renderer and not the renderer.Element
						var platform = renderer.Element.Platform as Platform;
						platform.DisposeRendererAndChildren(renderer);
						renderer = GetNewRenderer();
					}
				}

				Platform.SetRenderer(_viewCell.View, renderer);
			}
		}
	}
}
