using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

#if WINDOWS_UWP
using WLine = Microsoft.UI.Xaml.Shapes.Line;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WLine = System.Windows.Shapes.Line;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	public class LineRenderer : ShapeRenderer<Line, WLine>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Line> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new WLine());
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdateX1();
				UpdateY1();
				UpdateX2();
				UpdateY2();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == Line.X1Property.PropertyName)
				UpdateX1();
			else if (args.PropertyName == Line.Y1Property.PropertyName)
				UpdateY1();
			else if (args.PropertyName == Line.X2Property.PropertyName)
				UpdateX2();
			else if (args.PropertyName == Line.Y2Property.PropertyName)
				UpdateY2();
		}

		void UpdateX1()
		{
			Control.X1 = Element.X1;
		}

		void UpdateY1()
		{
			Control.Y1 = Element.Y1;
		}

		void UpdateX2()
		{
			Control.X2 = Element.X2;
		}

		void UpdateY2()
		{
			Control.Y2 = Element.Y2;
		}
	}
}
