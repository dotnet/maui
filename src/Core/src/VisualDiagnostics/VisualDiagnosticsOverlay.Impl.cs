using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsOverlay : WindowOverlay, IVisualDiagnosticsOverlay
	{
		private bool _enableElementSelector;

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualDiagnosticsOverlay"/> class.
		/// </summary>
		/// <param name="window">The parent Window.</param>
		public VisualDiagnosticsOverlay(IWindow window)
			: base(window)
		{
			this.OnTouch += VisualDiagnosticsOverlay_OnTouch;
		}

		/// <inheritdoc/>
		public bool AutoScrollToElement { get; set; }

		/// <inheritdoc/>
		public bool EnableElementSelector {
			get { return _enableElementSelector; }
			set
			{
				_enableElementSelector = value;
				this.DisableUITouchEventPassthrough = value;
				// If we enable the element picker,
				// make sure the view itself is enabled and visible.
				if (value)
					this.IsVisible = true;
			}
		}

		/// <inheritdoc/>
		public Point Offset { get; internal set; }

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
		public bool AddAdorner(IAdornerBorder adornerBorder, bool scrollToView = false)
		{
			this.AddScrollableElementHandlers();
			var result = this._windowElements.Add(adornerBorder);

			if (this.AutoScrollToElement || scrollToView)
				this.ScrollToView((IVisualTreeElement)adornerBorder.VisualView);

			this.Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public bool AddAdorner(IVisualTreeElement visualElement, bool scrollToView = false)
		{
			if (visualElement is not IView view)
				return false;

			var result = this._windowElements.Add(new RectangleGridAdornerBorder(view, this.DPI, this.Offset));
			this.AddScrollableElementHandlers();

			if (this.AutoScrollToElement || scrollToView)
				this.ScrollToView(visualElement);

			this.Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public bool RemoveAdorner(IAdornerBorder adornerBorder)
		{
			var results = this._windowElements.RemoveWhere(n => n == adornerBorder);
			if (!this._windowElements.Any())
				this.RemoveScrollableElementHandler();
			this.Invalidate();
			return results > 0;
		}

		/// <inheritdoc/>
		public void RemoveAdorners()
		{
			this.RemoveScrollableElementHandler();
			this._windowElements.Clear();
			this.Invalidate();
		}

		/// <inheritdoc/>
		public bool RemoveAdorners(IVisualTreeElement visualElement)
		{
			if (visualElement is not IView view)
				return false;

			var adorners = this._windowElements.Where(n => n is IAdornerBorder).Cast<IAdornerBorder>().Where(n => n.VisualView == view);
			var results = this._windowElements.RemoveWhere(n => adorners.Contains(n));
			this.Invalidate();
			return results > 0;
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

		/// <inheritdoc/>
		public override bool AddWindowElement(IWindowOverlayElement drawable)
		{
			if (drawable is not IAdornerBorder border)
				return false;
			return this.AddAdorner(border, this.AutoScrollToElement);
		}

		/// <inheritdoc/>
		public override bool RemoveWindowElement(IWindowOverlayElement drawable)
		{
			if (drawable is not IAdornerBorder border)
				return false;
			return this.RemoveAdorner(border);
		}

		/// <inheritdoc/>
		public override void RemoveWindowElements() => this.RemoveAdorners();

		protected override void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					this.RemoveScrollableElementHandler();
					this.OnTouch -= this.VisualDiagnosticsOverlay_OnTouch;
				}
			}

			base.Dispose(disposing);

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

		private void VisualDiagnosticsOverlay_OnTouch(object? sender, VisualDiagnosticsHitEvent e)
		{
			if (!this.EnableElementSelector)
				return;

			this.RemoveAdorners();
			if (e.VisualTreeElements.Any())
				this.AddAdorner(e.VisualTreeElements.First());
		}

#if NETSTANDARD || NET6

		/// <inheritdoc/>
		public IReadOnlyCollection<Tuple<IScrollView, object>> ScrollViews { get; } = new List<Tuple<IScrollView, object>>().AsReadOnly();

		/// <inheritdoc/>
		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
		}

		/// <summary>
		/// Disposes the native event hooks and handlers used to drive the overlay.
		/// </summary>
		private void DisposeNativeDependencies()
		{
		}
#endif
	}
}
