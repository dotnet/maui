using Xamarin.Forms;

namespace Microsoft.Maui.Graphics.Forms
{
	public class SkiaGraphicsView : View
	{
		public static readonly BindableProperty DrawableProperty = BindableProperty.Create(
			nameof(Drawable),
			typeof(IDrawable),
			typeof(SkiaGraphicsView),
			null);

		public IDrawable Drawable
		{
			get => (IDrawable) GetValue(DrawableProperty);
			set => SetValue(DrawableProperty, value);
		}
	}
}
