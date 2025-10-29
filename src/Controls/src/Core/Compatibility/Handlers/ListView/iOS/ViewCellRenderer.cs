#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ViewCellRenderer : CellRenderer
	{
		[Preserve(Conditional = true)]
		public ViewCellRenderer()
		{
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var viewCell = (ViewCell)item;
#pragma warning restore CS0618 // Type or member is obsolete

			var cell = reusableCell as ViewTableCell;
			if (cell == null)
				cell = new ViewTableCell(item.GetType().FullName);

			cell.ViewCell = viewCell;

			SetRealCell(item, cell);

			UpdateBackground(cell, item);

			SetAccessibility(cell, item);

			return cell;
		}

		internal sealed class ViewTableCell : UITableViewCell, INativeElementView
		{
			IMauiContext MauiContext => ViewCell?.FindMauiContext();
			WeakReference<IPlatformViewHandler> _rendererRef;
#pragma warning disable CS0618 // Type or member is obsolete
			WeakReference<ViewCell> _viewCell;
#pragma warning restore CS0618 // Type or member is obsolete

			Element INativeElementView.Element => ViewCell;
			internal bool SupressSeparator { get; set; }
			bool _disposed;

			public ViewTableCell(string key) : base(UITableViewCellStyle.Default, key)
			{
			}

#pragma warning disable CS0618 // Type or member is obsolete
			public ViewCell ViewCell
#pragma warning restore CS0618 // Type or member is obsolete
			{
				get => _viewCell?.GetTargetOrDefault();
				set
				{
					if (_viewCell is not null)
					{
						if (_viewCell.TryGetTarget(out var viewCell) && viewCell == value)
							return;
						viewCell?.Handler?.DisconnectHandler();
					}
					UpdateCell(value);
				}
			}

			void UpdateIsEnabled(bool isEnabled)
			{
				UserInteractionEnabled = isEnabled;
#pragma warning disable CA1416, CA1422 // TODO: TextLabel is unsupported on: 'ios' 14.0 and later
				TextLabel.Enabled = isEnabled;
#pragma warning restore C1416, CA1422
			}

			void ViewCellPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				var viewCell = (ViewCell)sender;
#pragma warning restore CS0618 // Type or member is obsolete
				var realCell = (ViewTableCell)GetRealCell(viewCell);

#pragma warning disable CS0618 // Type or member is obsolete
				if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
					UpdateIsEnabled(viewCell.IsEnabled);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			public override void LayoutSubviews()
			{
				//This sets the content views frame.
				base.LayoutSubviews();

				//TODO: Determine how best to hide the separator line when there is an accessory on the cell
				if (SupressSeparator && Accessory == UITableViewCellAccessory.None)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var oldFrame = Frame;
#pragma warning restore CS0618 // Type or member is obsolete
					ContentView.Bounds = new RectangleF(oldFrame.Location, new SizeF(oldFrame.Width, oldFrame.Height + 0.5f));
				}

				if (_rendererRef == null)
					return;

				if (_rendererRef.TryGetTarget(out IPlatformViewHandler handler))
				{
					var contentFrame = ContentView.Frame;
					handler.LayoutVirtualView(new RectangleF(0, 0, contentFrame.Width, contentFrame.Height));
				}
			}

			public override SizeF SizeThatFits(SizeF size)
			{
				if (!_rendererRef.TryGetTarget(out IPlatformViewHandler handler))
					return base.SizeThatFits(size);

				if (handler.VirtualView == null)
					return SizeF.Empty;

				double width = size.Width;
				var height = size.Height > 0 ? size.Height : double.PositiveInfinity;
				var result = handler.MeasureVirtualView(new SizeF(width, height));
				if (result == null)
					return base.SizeThatFits(size);

				// make sure to add in the separator if needed
				var finalheight = (float)result.Value.Height + (SupressSeparator ? 0f : 1f) / UIScreen.MainScreen.Scale;

				return new SizeF(size.Width, finalheight);
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing)
				{
					IPlatformViewHandler renderer;
					if (_rendererRef != null && _rendererRef.TryGetTarget(out renderer) && renderer.VirtualView != null)
					{
						renderer.VirtualView.DisposeModalAndChildHandlers();
						_rendererRef = null;
					}

#pragma warning disable CS0618 // Type or member is obsolete
					if (ViewCell is ViewCell viewCell)
					{
						viewCell.PropertyChanged -= ViewCellPropertyChanged;
						viewCell.View.MeasureInvalidated -= OnMeasureInvalidated;
						SetRealCell(viewCell, null);
					}
#pragma warning restore CS0618 // Type or member is obsolete
					_viewCell = null;
				}

				_disposed = true;

				base.Dispose(disposing);
			}

			IPlatformViewHandler GetNewRenderer()
			{
#pragma warning disable CS0618 // Type or member is obsolete
				if (ViewCell is not ViewCell viewCell || viewCell.View == null)
					throw new InvalidOperationException($"ViewCell must have a {nameof(viewCell.View)}");
#pragma warning restore CS0618 // Type or member is obsolete

				var newRenderer = viewCell.View.ToHandler(viewCell.View.FindMauiContext());
				_rendererRef = new WeakReference<IPlatformViewHandler>(newRenderer);
				ContentView.ClearSubviews();
				ContentView.AddSubview(newRenderer.VirtualView.ToPlatform());
				return (IPlatformViewHandler)newRenderer;
			}

#pragma warning disable CS0618 // Type or member is obsolete
			void UpdateCell(ViewCell cell)
#pragma warning restore CS0618 // Type or member is obsolete
			{
#pragma warning disable CS0618 // Type or member is obsolete
				if (ViewCell is ViewCell oldCell)
				{
					BeginInvokeOnMainThread(oldCell.SendDisappearing);
					oldCell.PropertyChanged -= ViewCellPropertyChanged;
					oldCell.View.MeasureInvalidated -= OnMeasureInvalidated;
				}
#pragma warning restore CS0618 // Type or member is obsolete

				if (cell is null)
				{
					_viewCell = null;
					_rendererRef = null;
					ContentView.ClearSubviews();
					return;
				}

				_viewCell = new(cell);

				cell.PropertyChanged += ViewCellPropertyChanged;
				BeginInvokeOnMainThread(cell.SendAppearing);

				IPlatformViewHandler renderer;
				if (_rendererRef == null || !_rendererRef.TryGetTarget(out renderer))
					renderer = GetNewRenderer();
				else
				{
					var viewHandlerType = MauiContext.Handlers.GetHandlerType(cell.View.GetType());
					var reflectableType = renderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : (renderer != null ? renderer.GetType() : typeof(System.Object));

					if (rendererType == viewHandlerType/* || (renderer is Platform.DefaultRenderer && type == null)*/)
						renderer.SetVirtualView(cell.View);
					else
					{
						//when cells are getting reused the element could be already set to another cell
						//so we should dispose based on the renderer and not the renderer.Element
						renderer.DisposeHandlersAndChildren();
						renderer = GetNewRenderer();
					}
				}

				UpdateIsEnabled(cell.IsEnabled);
				cell.View.MeasureInvalidated += OnMeasureInvalidated;
			}

			void OnMeasureInvalidated(object sender, EventArgs e)
			{
				SetNeedsLayout();
			}


		}
	}
}
