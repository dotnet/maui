using Microsoft.Maui.Graphics.SharpDX.WindowsForms;
using System.Windows.Forms.Integration;

namespace Microsoft.Maui.Graphics.SharpDX.WPF
{
	public class WDGraphicsView : WindowsFormsHost
	{
		private readonly WFGraphicsView _graphicsView;

		public WDGraphicsView() : this(null, null)
		{
		}

		public WDGraphicsView(IDrawable drawable = null, IGraphicsRenderer renderer = null)
		{
			_graphicsView = new WFGraphicsView(drawable, renderer);
			Child = _graphicsView;
		}

		public WFGraphicsView GraphicsView => _graphicsView;

		public IGraphicsRenderer Renderer
		{
			get => _graphicsView.Renderer;
			set => _graphicsView.Renderer = value;
		}

		public IDrawable Drawable
		{
			get => _graphicsView.Drawable;
			set => _graphicsView.Drawable = value;
		}

		public Color BackgroundColor
		{
			get => _graphicsView.BackgroundColor;
			set => _graphicsView.BackgroundColor = value;
		}

		public void Invalidate()
		{
			if (!_graphicsView.Dirty)
			{
				_graphicsView.Dirty = true;
				Dispatcher.Invoke(_graphicsView.Refresh);
			}
		}
	}
}
