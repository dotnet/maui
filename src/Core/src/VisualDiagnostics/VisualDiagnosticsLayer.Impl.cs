using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsLayer : IVisualDiagnosticsLayer, IDrawable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VisualDiagnosticsLayer"/> class.
		/// </summary>
		/// <param name="window">The parent Window.</param>
		public VisualDiagnosticsLayer(IWindow window)
		{
			this.Window = window;
		}

#pragma warning disable CS0067 // The event is never used
		/// <inheritdoc/>
		public event EventHandler<VisualDiagnosticsHitEvent>? OnTouch;
#pragma warning restore CS0067

		/// <inheritdoc/>
		public bool IsNativeViewInitialized { get; internal set; }

		/// <inheritdoc/>
		public IWindow Window { get; internal set; }

		/// <inheritdoc/>
		public HashSet<IAdornerBorder> AdornerBorders { get; internal set; } = new HashSet<IAdornerBorder>();

		/// <inheritdoc/>
		public Rectangle Offset { get; internal set; }

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
		public void AddAdorner(IAdornerBorder adornerBorder)
		{
			this.AddScrollableElementHandlers();
			this.AdornerBorders.Add(adornerBorder);
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void AddAdorner(IVisualTreeElement visualElement)
		{
			if (visualElement is not IView view)
			{
				return;
			}

			this.AdornerBorders.RemoveWhere(n => n.VisualView == view);
			this.AdornerBorders.Add(new RectangleGridAdornerBorder(view, this.DPI, this.Offset));
			this.AddScrollableElementHandlers();
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void RemoveAdorner(IAdornerBorder adornerBorder)
		{
			this.AdornerBorders.RemoveWhere(n => n == adornerBorder);
			if (!this.AdornerBorders.Any())
				this.RemoveScrollableElementHandler();
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void RemoveAdorners()
		{
			this.RemoveScrollableElementHandler();
			this.AdornerBorders.Clear();
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void RemoveAdorners(IVisualTreeElement visualElement)
		{
			if (visualElement is not IView view)
				return;

			this.AdornerBorders.RemoveWhere(n => n.VisualView == view);
			this.Invalidate();
		}

		/// <inheritdoc/>
		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			foreach (var border in this.AdornerBorders) 
				border.Draw(canvas, dirtyRect);
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

#if NETSTANDARD || NET6

		/// <inheritdoc/>
		public bool DisableUITouchEventPassthrough { get; set; }

		/// <inheritdoc/>
		public HashSet<Tuple<IScrollView, object>> ScrollViews { get; } = new HashSet<Tuple<IScrollView, object>>();

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
