using Cairo;
using Gdk;
using Gtk;
using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class BoxView : VBox
    {
        private BoxViewDrawingArea _boxView;

        public BoxView()
        {
            // Custom control created with Cairo. It gives nice possibilities as rounded corners, etc.
            _boxView = new BoxViewDrawingArea();

            Add(_boxView);
        }

        public void UpdateColor(Gdk.Color color)
        {
            if (_boxView != null)
            {
                _boxView.Color = color;
            }
        }

        public void ResetColor()
        {
            UpdateColor(Gdk.Color.Zero);
        }

        public void UpdateBackgroundColor(Gdk.Color color)
        {
            if (_boxView != null)
            {
                _boxView.ModifyBg(StateType.Normal, color);
            }
        }

        public void UpdateSize(int height, int width)
        {
            if (_boxView != null)
            {
                _boxView.Height = height;
                _boxView.Width = width;
            }
        }

        public void UpdateHasBorderRadius(bool hasBorderRadius)
        {
            if (_boxView != null)
            {
                _boxView.Radius = hasBorderRadius ? 5 : 0;
            }
        }
    }

    public class BoxViewDrawingArea : DrawingArea
    {
        private Context _context;
        private EventExpose _event;
        private Gdk.Color _color;
        private int _height;
        private int _width;
        private int _radius;

        public BoxViewDrawingArea()
        {
            QueueResize();
        }

        public Gdk.Color Color
        {
            get { return (_color); }
            set
            {
                _color = value;
                QueueDraw();
            }
        }

        public int Height
        {
            get { return (_height); }
            set
            {
                _height = value;
                QueueDraw();
            }
        }
        public int Width
        {
            get { return (_width); }
            set
            {
                _width = value;
                QueueDraw();
            }
        }

        public int Radius
        {
            get { return (_radius); }
            set
            {
                _radius = value;
                QueueDraw();
            }
        }

        protected override bool OnExposeEvent(EventExpose ev)
        {
            using (var cr = CairoHelper.Create(GdkWindow))
            {
                Draw(cr, ev);
            }

            return (true);
        }

        private void Draw(Context cr, EventExpose ev)
        {
            _context = cr;
            _event = ev;

            DrawBoxView(_context, _event);
        }

        private void DrawBoxView(Context cr, EventExpose ev)
        {
            if (Color.Equal(Gdk.Color.Zero)) return;

            int clipHeight = ev.Area.Height > 0 ? Height : 0;
            int clipWidth = ev.Area.Width > 0 ? Width : 0;

            double radius = Radius;
            int x = 0;
            int y = 0;
            int width = Width;
            int height = Height;

            cr.Rectangle(
                radius,
                0,
                clipHeight,
                clipWidth);

            cr.MoveTo(x, y);
            cr.Arc(x + width - radius, y + radius, radius, Math.PI * 1.5, Math.PI * 2);
            cr.Arc(x + width - radius, y + height - radius, radius, 0, Math.PI * .5);
            cr.Arc(x + radius, y + height - radius, radius, Math.PI * .5, Math.PI);
            cr.Arc(x + radius, y + radius, radius, Math.PI, Math.PI * 1.5);

            var cairoColor = new Cairo.Color(
               (double)Color.Red / ushort.MaxValue,
               (double)Color.Green / ushort.MaxValue,
               (double)Color.Blue / ushort.MaxValue);

            cr.SetSourceRGB(cairoColor.R, cairoColor.G, cairoColor.B);

            cr.Fill();
        }
    }
}