using System;
using System.ComponentModel;
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Internals;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class ViewCellRenderer : CellRenderer
	{
		[Preserve(Conditional = true)]
		public ViewCellRenderer()
		{
		}

		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			Performance.Start(out string reference);

			var viewCell = (ViewCell)item;

			var cell = reusableCell as ViewTableCell;
			if (cell == null)
				cell = new ViewTableCell(item.GetType().FullName);

			cell.ViewCell = viewCell;

			SetRealCell(item, cell);

			WireUpForceUpdateSizeRequested(item, cell, tv);

			UpdateBackground(cell, item);

			SetAccessibility(cell, item);

			Performance.Stop(reference);
			return cell;
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

			void UpdateIsEnabled(bool isEnabled)
			{
				UserInteractionEnabled = isEnabled;
				TextLabel.Enabled = isEnabled;
			}

			void ViewCellPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				var viewCell = (ViewCell)sender;
				var realCell = (ViewTableCell)GetRealCell(viewCell);

				if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
					UpdateIsEnabled(_viewCell.IsEnabled);
			}

			public override void LayoutSubviews()
			{
				Performance.Start(out string reference);

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

				Performance.Stop(reference);
			}

			public override SizeF SizeThatFits(SizeF size)
			{
				Performance.Start(out string reference);

				IVisualElementRenderer renderer;
				if (!_rendererRef.TryGetTarget(out renderer))
					return base.SizeThatFits(size);

				if (renderer.Element == null)
					return SizeF.Empty;

				double width = size.Width;
				var height = size.Height > 0 ? size.Height : double.PositiveInfinity;
				var result = renderer.Element.Measure(width, height, MeasureFlags.IncludeMargins);

				// make sure to add in the separator if needed
				var finalheight = (float)result.Request.Height + (SupressSeparator ? 0f : 1f) / UIScreen.MainScreen.Scale;

				Performance.Stop(reference);

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
						renderer.Element.DisposeModalAndChildRenderers();
						_rendererRef = null;
					}

					if (_viewCell != null)
					{
						_viewCell.PropertyChanged -= ViewCellPropertyChanged;
						_viewCell.View.MeasureInvalidated -= OnMeasureInvalidated;
						SetRealCell(_viewCell, null);
					}
					_viewCell = null;
				}

				_disposed = true;

				base.Dispose(disposing);
			}

			IVisualElementRenderer GetNewRenderer()
			{
				if (_viewCell.View == null)
					throw new InvalidOperationException($"ViewCell must have a {nameof(_viewCell.View)}");

				var newRenderer = Platform.CreateRenderer(_viewCell.View);
				_rendererRef = new WeakReference<IVisualElementRenderer>(newRenderer);
				ContentView.AddSubview(newRenderer.NativeView);
				return newRenderer;
			}

			void UpdateCell(ViewCell cell)
			{
				Performance.Start(out string reference);

				var oldCell = _viewCell;
				if (oldCell != null)
				{
					Device.BeginInvokeOnMainThread(oldCell.SendDisappearing);
					oldCell.PropertyChanged -= ViewCellPropertyChanged;
					oldCell.View.MeasureInvalidated -= OnMeasureInvalidated;
				}

				_viewCell = cell;
				_viewCell.PropertyChanged += ViewCellPropertyChanged;
				Device.BeginInvokeOnMainThread(_viewCell.SendAppearing);

				IVisualElementRenderer renderer;
				if (_rendererRef == null || !_rendererRef.TryGetTarget(out renderer))
					renderer = GetNewRenderer();
				else
				{
					if (renderer.Element != null && renderer == Platform.GetRenderer(renderer.Element))
						renderer.Element.ClearValue(Platform.RendererProperty);

					var type = Microsoft.Maui.Controls.Internals.Registrar.Registered.GetHandlerTypeForObject(this._viewCell.View);
					var reflectableType = renderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : renderer.GetType();
					if (rendererType == type || (renderer is Platform.DefaultRenderer && type == null))
						renderer.SetElement(this._viewCell.View);
					else
					{
						//when cells are getting reused the element could be already set to another cell
						//so we should dispose based on the renderer and not the renderer.Element
						renderer.DisposeRendererAndChildren();

						renderer = GetNewRenderer();
					}
				}

				Platform.SetRenderer(_viewCell.View, renderer);
				UpdateIsEnabled(_viewCell.IsEnabled);
				_viewCell.View.MeasureInvalidated += OnMeasureInvalidated;
				Performance.Stop(reference);
			}

			void OnMeasureInvalidated(object sender, EventArgs e)
			{
				SetNeedsLayout();
			}

			
		}
	}
}
