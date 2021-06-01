using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class GraphicsView : View, IGraphicsView
	{
		public static readonly BindableProperty DrawableProperty =
			BindableProperty.Create(nameof(Drawable), typeof(IDrawable), typeof(GraphicsView), null);

		public IDrawable Drawable
		{
			set { SetValue(DrawableProperty, value); }
			get { return (IDrawable)GetValue(DrawableProperty); }
		}
	}
}