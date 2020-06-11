using FormsRect = Xamarin.Forms.Rectangle;

namespace Xamarin.Forms.Shapes
{
    public class RectangleGeometry : Geometry
    {
        public static readonly BindableProperty RectProperty =
            BindableProperty.Create(nameof(Rect), typeof(FormsRect), typeof(RectangleGeometry), new FormsRect());

        public FormsRect Rect
        {
            set { SetValue(RectProperty, value); }
            get { return (FormsRect)GetValue(RectProperty); }
        }
    }
}