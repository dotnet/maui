using System.Windows.Shapes;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class BoxViewRenderer : ViewRenderer<BoxView, System.Windows.Shapes.Rectangle>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			var rect = new System.Windows.Shapes.Rectangle();
			rect.DataContext = Element;
			rect.SetBinding(Shape.FillProperty, new System.Windows.Data.Binding("Color") { Converter = new ColorConverter() });
			SetNativeControl(rect);
		}
	}
}