using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Controls.Platform;

#if WINDOWS
using WEllipse = Microsoft.UI.Xaml.Shapes.Ellipse;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WEllipse = System.Windows.Shapes.Ellipse;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	public class EllipseRenderer : ShapeRenderer<Ellipse, WEllipse>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Ellipse> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new WEllipse());
			}

			base.OnElementChanged(args);
		}
	}
}
