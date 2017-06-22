using Android.Content;
using Android.Views;
using AView = Android.Views.View;
using Xamarin.Forms.Internals;
using System;

namespace Xamarin.Forms.Platform.Android
{
	public class ViewCellRenderer : CellRenderer
	{
		protected override AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			Performance.Start();
			var cell = (ViewCell)item;

			var container = convertView as ViewCellContainer;
			if (container != null)
			{
				container.Update(cell);
				Performance.Stop();
				return container;
			}

			BindableProperty unevenRows = null, rowHeight = null;
			if (ParentView is TableView)
			{
				unevenRows = TableView.HasUnevenRowsProperty;
				rowHeight = TableView.RowHeightProperty;
			}
			else if (ParentView is ListView)
			{
				unevenRows = ListView.HasUnevenRowsProperty;
				rowHeight = ListView.RowHeightProperty;
			}

			if (cell.View == null)
				throw new InvalidOperationException($"ViewCell must have a {nameof(cell.View)}");

			IVisualElementRenderer view = Platform.CreateRenderer(cell.View);
			Platform.SetRenderer(cell.View, view);
			cell.View.IsPlatformEnabled = true;
			var c = new ViewCellContainer(context, view, cell, ParentView, unevenRows, rowHeight);

			Performance.Stop();

			return c;
		}

		internal class ViewCellContainer : ViewGroup, INativeElementView
		{
			readonly View _parent;
			readonly BindableProperty _rowHeight;
			readonly BindableProperty _unevenRows;
			IVisualElementRenderer _view;
			ViewCell _viewCell;

			public ViewCellContainer(Context context, IVisualElementRenderer view, ViewCell viewCell, View parent, BindableProperty unevenRows, BindableProperty rowHeight) : base(context)
			{
				_view = view;
				_parent = parent;
				_unevenRows = unevenRows;
				_rowHeight = rowHeight;
				_viewCell = viewCell;
				AddView(view.View);
				UpdateIsEnabled();
				UpdateLongClickable();
			}

			protected bool ParentHasUnevenRows
			{
				get { return (bool)_parent.GetValue(_unevenRows); }
			}

			protected int ParentRowHeight
			{
				get { return (int)_parent.GetValue(_rowHeight); }
			}

			public Element Element
			{
				get { return _viewCell; }
			}

			public override bool OnInterceptTouchEvent(MotionEvent ev)
			{
				if (!Enabled)
					return true;

				return base.OnInterceptTouchEvent(ev);
			}

			public void Update(ViewCell cell)
			{
				Performance.Start();

				var renderer = GetChildAt(0) as IVisualElementRenderer;
				var viewHandlerType = Registrar.Registered.GetHandlerType(cell.View.GetType()) ?? typeof(Platform.DefaultRenderer);
				if (renderer != null && renderer.GetType() == viewHandlerType)
				{
					Performance.Start("Reuse");
					_viewCell = cell;

					cell.View.DisableLayout = true;
					foreach (VisualElement c in cell.View.Descendants())
						c.DisableLayout = true;

					Performance.Start("Reuse.SetElement");
					renderer.SetElement(cell.View);
					Performance.Stop("Reuse.SetElement");

					Platform.SetRenderer(cell.View, _view);

					cell.View.DisableLayout = false;
					foreach (VisualElement c in cell.View.Descendants())
						c.DisableLayout = false;

					var viewAsLayout = cell.View as Layout;
					if (viewAsLayout != null)
						viewAsLayout.ForceLayout();

					Invalidate();

					Performance.Stop("Reuse");
					Performance.Stop();
					return;
				}

				RemoveView(_view.View);
				Platform.SetRenderer(_viewCell.View, null);
				_viewCell.View.IsPlatformEnabled = false;
				_view.View.Dispose();

				_viewCell = cell;
				_view = Platform.CreateRenderer(_viewCell.View);

				Platform.SetRenderer(_viewCell.View, _view);
				AddView(_view.View);

				UpdateIsEnabled();
				UpdateLongClickable();

				Performance.Stop();
			}

			public void UpdateIsEnabled()
			{
				Enabled = _viewCell.IsEnabled;
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				Performance.Start();

				double width = Context.FromPixels(r - l);
				double height = Context.FromPixels(b - t);

				Performance.Start("Element.Layout");
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(_view.Element, new Rectangle(0, 0, width, height));
				Performance.Stop("Element.Layout");

				_view.UpdateLayout();
				Performance.Stop();
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				Performance.Start();

				int width = MeasureSpec.GetSize(widthMeasureSpec);
				int height;

				if (ParentHasUnevenRows)
				{
					SizeRequest measure = _view.Element.Measure(Context.FromPixels(width), double.PositiveInfinity, MeasureFlags.IncludeMargins);
					height = (int)Context.ToPixels(_viewCell.Height > 0 ? _viewCell.Height : measure.Request.Height);
				}
				else
					height = (int)Context.ToPixels(ParentRowHeight == -1 ? BaseCellView.DefaultMinHeight : ParentRowHeight);

				SetMeasuredDimension(width, height);

				Performance.Stop();
			}

			void UpdateLongClickable()
			{
				// In order for context menu long presses/clicks to work on ViewCells which have 
				// and Clickable content, we have to make the container view LongClickable
				// If we don't have a context menu, we don't have to worry about it
				_view.View.LongClickable = _viewCell.ContextActions.Count > 0;
			}
		}
	}
}