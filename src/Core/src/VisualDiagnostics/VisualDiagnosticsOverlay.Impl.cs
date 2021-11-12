using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsOverlay : IVisualDiagnosticsOverlay, IDrawable
	{
		private HashSet<IAdornerBorder> _adornerBorders = new HashSet<IAdornerBorder>();

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualDiagnosticsOverlay"/> class.
		/// </summary>
		/// <param name="window">The parent Window.</param>
		public VisualDiagnosticsOverlay(IWindow window)
		{
			this.Window = window;
		}

#pragma warning disable CS0067 // The event is never used
		/// <inheritdoc/>
		public event EventHandler<VisualDiagnosticsHitEvent>? OnTouch;
#pragma warning restore CS0067

		/// <inheritdoc/>
		public bool AutoScrollToElement { get; set; }

		/// <inheritdoc/>
		public bool IsNativeViewInitialized { get; internal set; }

		/// <inheritdoc/>
		public IWindow Window { get; internal set; }

		/// <inheritdoc/>
		public IReadOnlyCollection<IAdornerBorder> AdornerBorders => this._adornerBorders.ToList().AsReadOnly();

		/// <inheritdoc/>
		public Point Offset { get; internal set; }

		/// <inheritdoc/>
		public float DPI { get; internal set; } = 1;

		public void AddScrollableElementHandlers()
		{
			var scrollBars = this.GetScrollViews();
			foreach (var scrollBar in scrollBars)
			{
				if (!this.ScrollViews.Any(x => x.Item1 == scrollBar))
				{
					this.AddScrollableElementHandler(scrollBar);
				}
			}
		}

		/// <inheritdoc/>
		public void AddAdorner(IAdornerBorder adornerBorder, bool scrollToView = false)
		{
			this.AddScrollableElementHandlers();
			this._adornerBorders.Add(adornerBorder);

			if (this.AutoScrollToElement || scrollToView)
				this.ScrollToView((IVisualTreeElement)adornerBorder.VisualView);

			this.Invalidate();
		}

		/// <inheritdoc/>
		public void AddAdorner(IVisualTreeElement visualElement, bool scrollToView = false)
		{
			if (visualElement is not IView view)
				return;

			this._adornerBorders.RemoveWhere(n => n.VisualView == view);
			this._adornerBorders.Add(new RectangleGridAdornerBorder(view, this.DPI, this.Offset));
			this.AddScrollableElementHandlers();

			if (this.AutoScrollToElement || scrollToView)
				this.ScrollToView(visualElement);

			this.Invalidate();
		}

		/// <inheritdoc/>
		public void RemoveAdorner(IAdornerBorder adornerBorder)
		{
			this._adornerBorders.RemoveWhere(n => n == adornerBorder);
			if (!this._adornerBorders.Any())
				this.RemoveScrollableElementHandler();
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void RemoveAdorners()
		{
			this.RemoveScrollableElementHandler();
			this._adornerBorders.Clear();
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void RemoveAdorners(IVisualTreeElement visualElement)
		{
			if (visualElement is not IView view)
				return;

			this._adornerBorders.RemoveWhere(n => n.VisualView == view);
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			foreach (var border in this._adornerBorders) 
				border.Draw(canvas, dirtyRect);
		}

		/// <inheritdoc/>
		public void ScrollToView(IVisualTreeElement element)
		{
			var parentScrollView = GetParentScrollView(element);
			if (parentScrollView == null)
				return;

			if (element is not IView view)
				return;

			var nativeView = view.GetNativeViewBounds();
			parentScrollView.RequestScrollTo(nativeView.X, nativeView.Y, true);
		}

		/// <summary>
		/// Gets the list of <see cref="IScrollView"/>, if any are in the visual element tree.
		/// </summary>
		/// <returns>List of IScrollView.</returns>
		internal List<IScrollView> GetScrollViews ()
		{
			if (this.Window == null)
				return new List<IScrollView>();
			var content = this.Window.Content as IVisualTreeElement;
			if (content == null)
				return new List<IScrollView>();

			return content.GetEntireVisualTreeElementChildren().Where(n => n is IScrollView).Cast<IScrollView>().ToList();
		}

		/// <summary>
		/// Handles <see cref="OnTouch"/> event.
		/// </summary>
		/// <param name="point">Point where user has touched.</param>
		/// <param name="addAdorner">
		/// If true, will get the visual tree to see if there are any elements for the given point,
		/// if so, it will add an adorner for the top most element.
		/// </param>
		internal void OnTouchInternal(Point point, bool addAdorner = false)
		{
			var elements = new List<IVisualTreeElement>();

			if (this.DisableUITouchEventPassthrough)
			{
				var visualWindow = this.Window as IVisualTreeElement;
				if (visualWindow != null)
					elements.AddRange(visualWindow.GetVisualTreeElements(point));
			}

			if (addAdorner && elements.Any())
			{
				this.AddAdorner(elements.First());
			}

			this.OnTouch?.Invoke(this, new VisualDiagnosticsHitEvent(point, elements));
		}

		private IScrollView? GetParentScrollView(IVisualTreeElement element)
		{
			if (element is IScrollView scrollView)
				return scrollView;
			if (element == null)
				return null;
			var parent = element.GetVisualParent();
			if (parent != null)
				return GetParentScrollView(parent);
			return null;
		}

#if NETSTANDARD || NET6

		/// <inheritdoc/>
		public bool DisableUITouchEventPassthrough { get; set; }

		/// <inheritdoc/>
		public IReadOnlyCollection<Tuple<IScrollView, object>> ScrollViews { get; } = new List<Tuple<IScrollView, object>>().AsReadOnly();

		/// <inheritdoc/>
		public void Invalidate()
		{
		}

		/// <inheritdoc/>
		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
		}
#endif
	}
}
