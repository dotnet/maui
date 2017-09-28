using Cairo;
using Gdk;
using Gtk;
using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class ActivityIndicator : EventBox
    {
        private const int DefaultHeight = 48;
        private const int DefaultWidth = 48;

        private ActivityIndicatorDrawingArea _activityIndicator;

        public ActivityIndicator()
        {
            HeightRequest = DefaultHeight;
            WidthRequest = DefaultWidth;

            // Custom control created with Cairo.
            _activityIndicator = new ActivityIndicatorDrawingArea
            {
                Height = HeightRequest,
                Width = WidthRequest
            };

            _activityIndicator.SetFlag(WidgetFlags.NoWindow);

            Add(_activityIndicator);
        }

        public void Start()
        {
            if (_activityIndicator != null)
            {
                _activityIndicator.Start();
            }
        }

        public void Stop()
        {
            if (_activityIndicator != null)
            {
                _activityIndicator.Stop();
            }
        }

        public void UpdateBackgroundColor(Gdk.Color backgroundColor)
        {
            if (_activityIndicator != null)
            {
                _activityIndicator.BackgroundColor = backgroundColor;
                _activityIndicator.ModifyBg(StateType.Normal, backgroundColor);
            }
        }

        public void UpdateAlpha(double alpha)
        {
            if (_activityIndicator != null)
            {
                _activityIndicator.Alpha = alpha;
            }
        }

        public void UpdateColor(Gdk.Color color)
        {
            if (_activityIndicator != null)
            {
                _activityIndicator.Color = color;
            }
        }
    }

    public class ActivityIndicatorDrawingArea : DrawingArea
    {
        private int _height;
        private int _width;
        private bool _running;
        private int _current;
        private int _lines;
        private Gdk.Color _color;
        private Gdk.Color _backgroundColor;
        private double _alpha;

        public ActivityIndicatorDrawingArea()
        {
            _height = 48;
            _width = 48;
            _current = 0;
            _lines = 8; // Number of lines in circular form.
            _running = false;

            QueueResize();
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

        public int Lines
        {
            get { return (_lines); }
            set
            {
                _lines = value;
                QueueDraw();
            }
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

        public Gdk.Color BackgroundColor
        {
            get { return (_backgroundColor); }
            set
            {
                _backgroundColor = value;
                QueueDraw();
            }
        }

        public double Alpha
        {
            get { return (_alpha); }
            set
            {
                _alpha = value;
                QueueDraw();
            }
        }

        public bool IsRunning
        {
            get { return (_running); }
        }

        public void Start()
        {
            _running = true;
            GLib.Timeout.Add(100, ExposeTimeoutHandler);    // Every 100 ms.
            QueueDraw();
        }

        public void Stop()
        {
            _running = false;
            QueueDraw();
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            base.OnExposeEvent(evnt);

            using (var cr = CairoHelper.Create(GdkWindow))
            {
                cr.Rectangle(
                    evnt.Area.X,
                    evnt.Area.Y,
                    Height,
                    Width);

                cr.Clip();

                Draw(cr);
            }

            return (true);
        }

        private bool ExposeTimeoutHandler()
        {
            if (_current + 1 > _lines)
            {
                _current = 0;
            }
            else
            {
                _current++;
            }
            QueueDraw();
            return (_running);
        }

        private void Draw(Context cr)
        {
            // Set BackgroundColor
            cr.Save();

            var cairoBackgroundColor = new Cairo.Color(
              (double)BackgroundColor.Red / ushort.MaxValue,
              (double)BackgroundColor.Green / ushort.MaxValue,
              (double)BackgroundColor.Blue / ushort.MaxValue);

            cr.SetSourceRGBA(cairoBackgroundColor.R, cairoBackgroundColor.G, cairoBackgroundColor.B, Alpha);
            cr.Paint();
            cr.Restore();

            // Draw Activity Indicator
            double radius;
            double half;
            double x, y;

            x = Allocation.X + Width / 2;
            y = Allocation.Y + _height / 2;

            radius = Math.Min(
                Width / 2,
                _height / 2);

            half = _lines / 2;

            for (int i = 0; i < _lines; i++)
            {
                double move = (double)((i + _lines - _current) % _lines) / _lines;
                double inset = 0.5 * radius;

                cr.Save();

                var cairoColor = new Cairo.Color(
                    (double)Color.Red / ushort.MaxValue,
                    (double)Color.Green / ushort.MaxValue,
                    (double)Color.Blue / ushort.MaxValue);

                cr.SetSourceRGBA(cairoColor.R, cairoColor.G, cairoColor.B, move);

                cr.LineWidth *= 2;
                cr.MoveTo(move + x + (radius - inset) * Math.Cos(i * Math.PI / half),
                          move + y + (radius - inset) * Math.Sin(i * Math.PI / half));
                cr.LineTo(x + radius * Math.Cos(i * Math.PI / half),
                          y + radius * Math.Sin(i * Math.PI / half));

                cr.Stroke();
                cr.Restore();
            }
        }
    }
}