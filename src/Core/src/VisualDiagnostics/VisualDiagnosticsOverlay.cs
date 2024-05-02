using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
#if ANDROID
	[System.Runtime.Versioning.SupportedOSPlatform("android23.0")]
#endif
	public partial class VisualDiagnosticsOverlay : WindowOverlay, IVisualDiagnosticsOverlay
	{
		bool _enableElementSelector;

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualDiagnosticsOverlay"/> class.
		/// </summary>
		/// <param name="window">The parent Window.</param>
		public VisualDiagnosticsOverlay(IWindow window)
			: base(window)
		{
			Tapped += VisualDiagnosticsOverlayOnTapped;
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<IScrollView> ScrollViews => _scrollViews.Keys;

		/// <inheritdoc/>
		public bool ScrollToElement { get; set; }

		/// <inheritdoc/>
		public bool EnableElementSelector
		{
			get => _enableElementSelector;
			set
			{
				_enableElementSelector = value;
				DisableUITouchEventPassthrough = value;

				// If we enable the element picker, make sure the view itself is enabled and visible.
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
				if (!ScrollViews.Contains(scrollBar))
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
			var result = base.AddWindowElement(adorner);

			if (ScrollToElement || scrollToView)
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

			foreach (var element in WindowElements)
			{
				if (element is IAdorner adorner && adorner.VisualView == view)
					return false;
			}

			var result = base.AddWindowElement(new RectangleGridAdorner(view, Density, Offset));
			AddScrollableElementHandlers();

			if (ScrollToElement || scrollToView)
				ScrollToView(visualElement);

			Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public bool RemoveAdorner(IAdorner adorner)
		{
			if (adorner == null)
				throw new ArgumentNullException(nameof(adorner));

			var result = base.RemoveWindowElement(adorner);
			if (WindowElements.Count == 0)
				RemoveScrollableElementHandler();

			Invalidate();
			return result;
		}

		/// <inheritdoc/>
		public void RemoveAdorners()
		{
			RemoveScrollableElementHandler();
			base.RemoveWindowElements();
		}

		/// <inheritdoc/>
		public bool RemoveAdorners(IVisualTreeElement visualElement)
		{
			if (visualElement == null)
				throw new ArgumentNullException(nameof(visualElement));

			if (visualElement is not IView view)
				return false;

			// make a copy because we will edit
			var removed = false;
			foreach (var element in WindowElements.ToList())
			{
				if (element is IAdorner adorner && adorner.VisualView == view)
					removed = base.RemoveWindowElement(element) || removed;
			}

			Invalidate();
			return removed;
		}

		/// <inheritdoc/>
		public void ScrollToView(IVisualTreeElement element)
		{
			var parentScrollView = GetParentScrollView(element);
			if (parentScrollView == null)
				return;

			if (element is not IView view)
				return;

			var platformView = view.GetPlatformViewBounds();
			parentScrollView.RequestScrollTo(platformView.X, platformView.Y, true);
		}

		/// <inheritdoc/>
		public override bool AddWindowElement(IWindowOverlayElement drawable)
		{
			if (drawable is not IAdorner adorner)
				return false;

			return AddAdorner(adorner, ScrollToElement);
		}

		/// <inheritdoc/>
		public override bool RemoveWindowElement(IWindowOverlayElement drawable)
		{
			if (drawable is not IAdorner adorner)
				return false;

			return RemoveAdorner(adorner);
		}

		/// <inheritdoc/>
		public override void RemoveWindowElements() =>
			RemoveAdorners();

		public override bool Deinitialize()
		{
			RemoveScrollableElementHandler();

			Tapped -= VisualDiagnosticsOverlayOnTapped;

			return base.Deinitialize();
		}

		List<IScrollView> GetScrollViews()
		{
			if (Window == null)
				return new List<IScrollView>();

			if (Window.Content is not IVisualTreeElement content)
				return new List<IScrollView>();

			return content.GetVisualTreeDescendants()
				.OfType<IScrollView>()
				.ToList();
		}

		static IScrollView? GetParentScrollView(IVisualTreeElement element)
		{
			if (element == null)
				return null;

			if (element is IScrollView scrollView)
				return scrollView;

			var parent = element.GetVisualParent();
			if (parent != null)
				return GetParentScrollView(parent);

			return null;
		}

		void VisualDiagnosticsOverlayOnTapped(object? sender, WindowOverlayTappedEventArgs e)
		{
			if (!EnableElementSelector)
				return;

			RemoveAdorners();

			if (e.VisualTreeElements.Any())
				AddAdorner(e.VisualTreeElements.First());
		}
	}
}