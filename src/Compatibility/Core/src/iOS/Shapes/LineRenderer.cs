using System;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Controls.Shapes;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
    public class LineRenderer : ShapeRenderer<Line, LineView>
    {
        [Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
        public LineRenderer()
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Line> args)
        {
            if (Control == null && args.NewElement != null)
            {
                SetNativeControl(new LineView());
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
            Control.UpdateX1(Element.X1);
        }

        void UpdateY1()
        {
            Control.UpdateY1(Element.Y1);
        }

        void UpdateX2()
        {
            Control.UpdateX2(Element.X2);
        }

        void UpdateY2()
        {
            Control.UpdateY2(Element.Y2);
        }
    }

    public class LineView : ShapeView
    {
        nfloat _x1, _y1, _x2, _y2;

        public void UpdateX1(double x1)
        {
            _x1 = new nfloat(x1);
            UpdateShape();
        }

        public void UpdateY1(double y1)
        {
            _y1 = new nfloat(y1);
            UpdateShape();
        }

        public void UpdateX2(double x2)
        {
            _x2 = new nfloat(x2);
            UpdateShape();
        }

        public void UpdateY2(double y2)
        {
            _y2 = new nfloat(y2);
            UpdateShape();
        }

        void UpdateShape()
        {
			var path = new CGPath();
            path.MoveToPoint(_x1, _y1);
            path.AddLineToPoint(_x2, _y2);
            ShapeLayer.UpdateShape(path);
        }
    }
}