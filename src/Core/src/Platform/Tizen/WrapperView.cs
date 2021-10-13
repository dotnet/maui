using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : NView
	{
		public WrapperView()
		{
		}

		public void UpdateBackground(Paint? paint)
		{
			UpdateDrawableCanvas(paint);
		}

		public void UpdateShape(IShape? shape)
		{
			UpdateDrawableCanvas(shape);
		}

		public void UpdateBorder(IBorderStroke border)
		{
			((MauiDrawable)_drawableCanvas.Value.Drawable).Border = border;
			UpdateShape(border.Shape);
		}

		public void UpdateBorder(IBorder border)
		{
			if (!_drawableCanvas.IsValueCreated && Shadow is null)
				return;

			((MauiDrawable)_drawableCanvas.Value.Drawable).Shadow = Shadow;

			if (Shadow != null)
			{
				_drawableCanvas.Value.SetClip(null);
			}
			UpdateDrawableCanvas(true);
		}

		partial void ClipChanged()
		{
		}
	}
}
