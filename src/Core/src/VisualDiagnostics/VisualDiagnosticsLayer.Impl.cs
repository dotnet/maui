using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsLayer : IVisualDiagnosticsLayer, IDrawable
	{
		

		public bool IsNativeViewInitialized { get; internal set; }

		public IWindow Window { get; internal set; }

		public HashSet<IAdornerBorder> AdornerBorders { get; internal set; } = new HashSet<IAdornerBorder>();

		public Rectangle Offset { get; internal set; }

		public float DPI { get; internal set; } = 1;

		public VisualDiagnosticsLayer(IWindow window)
		{
			this.Window = window;
		}

#pragma warning disable CS0067 // The event is never used
		public event EventHandler<VisualDiagnosticsHitEvent>? OnTouch;
#pragma warning restore CS0067

		public void AddAdorner(IAdornerBorder adornerBorder)
		{
			this.AdornerBorders.Add(adornerBorder);
			this.Invalidate();
		}

		public void AddAdorner(IVisualTreeElement visualElement)
		{
			if (visualElement is not IView view)
			{
				return;
			}

			this.AdornerBorders.RemoveWhere(n => n.VisualView == view);
			this.AdornerBorders.Add(new RectangleGridAdornerBorder(view, this.DPI, this.Offset));
			this.Invalidate();
		}

		public void RemoveAdorner(IAdornerBorder adornerBorder)
		{
			this.AdornerBorders.RemoveWhere(n => n == adornerBorder);
			this.Invalidate();
		}

		public void RemoveAdorners()
		{
			this.AdornerBorders.Clear();
			this.Invalidate();
		}

		public void RemoveAdorners(IVisualTreeElement visualElement)
		{
			if (visualElement is not IView view)
				return;

			this.AdornerBorders.RemoveWhere(n => n.VisualView == view);
			this.Invalidate();
		}


		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			foreach (var border in this.AdornerBorders) 
				border.Draw(canvas, dirtyRect);
		}

#if NETSTANDARD || NET6

		public bool DisableUITouchEventPassthrough { get; set; }

		public void Invalidate()
		{
		}
#endif
	}
}
