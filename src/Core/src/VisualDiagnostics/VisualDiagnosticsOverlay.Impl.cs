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
			OnTouch += VisualDiagnosticsOverlayOnTouch;
		}

		/// <inheritdoc/>
		public bool AutoScrollToElement { get; set; }

		/// <inheritdoc/>
		public bool EnableElementSelector {
			get { return _enableElementSelector; }
			set
			{
				_enableElementSelector = value;
				DisableUITouchEventPassthrough = value;
				// If we enable the element picker,
				// make sure the view itself is enabled and visible.
				if (value)
					IsVisible = true;
			}
		}

		/// <inheritdoc/>
		public Point Offset { get; internal set; }

		public void AddScrollableElementHandlers()
		{
			var scrollBars = GetScrollViews();
			foreach (var scrollBar in scrollBars)
			{
				if (!ScrollViews.ContainsKey(scrollBar))
				{
					AddScrollableElementHandler(scrollBar);
				}
			}
		}

		/// <inheritdoc/>
		public bool AddAdorner(IAdorner adorner, bool scrollToView = false)
		{
			if (adorner == null)
				throw new ArgumentNullException(nameof(adorner));

			AddScrollableElementHandlers();
			var result = _windowElements.Add(adorner);

			if (AutoScrollToElement || scrollToView)
				ScrollToView((IVisualTreeElement)adorner.VisualView);

			Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public bool AddAdorner(IVisualTreeElement visualElement, bool scrollToView = false)
		{
			if (visualElement == null)
				throw new ArgumentNullException(nameof(visualElement));

			if (visualElement is not IView view)
				return false;

			if (_windowElements.OfType<IAdorner>().Any(n => n.VisualView == view))
				return false;

			var result = _windowElements.Add(new RectangleGridAdorner(view, Density, Offset));
			AddScrollableElementHandlers();

			if (AutoScrollToElement || scrollToView)
				ScrollToView(visualElement);

			Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public bool RemoveAdorner(IAdorner adorner)
		{
			if (adorner == null)
				throw new ArgumentNullException(nameof(adorner));

			var results = _windowElements.RemoveWhere(n => n == adorner);
			if (!_windowElements.Any())
				RemoveScrollableElementHandler();
			Invalidate();
			return results > 0;
		}

		/// <inheritdoc/>
		public void RemoveAdorners()
		{
			RemoveScrollableElementHandler();
			_windowElements.Clear();
			Invalidate();
		}

		/// <inheritdoc/>
		public bool RemoveAdorners(IVisualTreeElement visualElement)
		{
			if (visualElement == null)
				throw new ArgumentNullException(nameof(visualElement));

			if (visualElement is not IView view)
				return false;

			var adorners = _windowElements.OfType<IAdorner>().Where(n => n.VisualView == view);
			var results = _windowElements.RemoveWhere(n => adorners.Contains(n));
			Invalidate();
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
			if (drawable is not IAdorner adorner)
				return false;
			return AddAdorner(adorner, AutoScrollToElement);
		}

		/// <inheritdoc/>
		public override bool RemoveWindowElement(IWindowOverlayElement drawable)
		{
			if (drawable is not IAdorner adorner)
				return false;
			return RemoveAdorner(adorner);
		}

		/// <inheritdoc/>
		public override void RemoveWindowElements() => RemoveAdorners();

		protected override void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					RemoveScrollableElementHandler();
					OnTouch -= VisualDiagnosticsOverlayOnTouch;
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
			if (Window == null)
				return new List<IScrollView>();
			var content = Window.Content as IVisualTreeElement;
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

		private void VisualDiagnosticsOverlayOnTouch(object? sender, VisualDiagnosticsHitEvent e)
		{
			if (!EnableElementSelector)
				return;

			RemoveAdorners();
			if (e.VisualTreeElements.Any())
				AddAdorner(e.VisualTreeElements.First());
		}

#if NETSTANDARD || NET6

		/// <inheritdoc/>
		public IReadOnlyDictionary<IScrollView, object> ScrollViews { get; } = new Dictionary<IScrollView, object>();

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
