using Gdk;
using Gtk;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class CustomFrame : Gtk.Frame
    {
        private Gdk.Color _defaultBorderColor;
        private Gdk.Color _defaultBackgroundColor;
        private Gdk.Color? _borderColor;
        private Gdk.Color? _backgroundColor;

        private uint _borderWidth;
        private bool _hasShadow;
        private uint _shadowWidth;

        public CustomFrame()
        {
            ShadowType = ShadowType.None;
            BorderWidth = 0;

            _borderWidth = 0;
            _hasShadow = false;
            _shadowWidth = 2;
            _defaultBackgroundColor = Style.Backgrounds[(int)StateType.Normal];
            _defaultBorderColor = Style.BaseColors[(int)StateType.Active];
        }

        public void SetBackgroundColor(Gdk.Color? color)
        {
            _backgroundColor = color;
            QueueDraw();
        }

        public void ResetBackgroundColor()
        {
            _backgroundColor = _defaultBackgroundColor;
            QueueDraw();
        }

        public void SetBorderWidth(uint width)
        {
            _borderWidth = width;
            QueueDraw();
        }

        public void SetBorderColor(Gdk.Color? color)
        {
            _borderColor = color;
            QueueDraw();
        }

        public void ResetBorderColor()
        {
            _borderColor = _defaultBorderColor;
            QueueDraw();
        }

        public void SetShadow()
        {
            _hasShadow = true;
            QueueDraw();
        }

        public void ResetShadow()
        {
            _hasShadow = false;
            QueueDraw();
        }

        public void SetShadowWidth(uint width)
        {
            _shadowWidth = width;
            QueueDraw();
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            double colorMaxValue = 65535;

            using (var cr = CairoHelper.Create(GdkWindow))
            {
                //Draw Shadow
                if (_hasShadow)
                {
                    var color = Color.Black.ToGtkColor();
                    cr.SetSourceRGBA(color.Red / colorMaxValue, color.Green / colorMaxValue, color.Blue / colorMaxValue, 1.0);
                    cr.Rectangle(Allocation.Left + _shadowWidth, Allocation.Top + _shadowWidth, Allocation.Width + _shadowWidth, Allocation.Height + _shadowWidth);
                    cr.Fill();
                }

                // Draw BackgroundColor
                if (_backgroundColor.HasValue)
                {
                    var color = _backgroundColor.Value;
                    cr.SetSourceRGBA(color.Red / colorMaxValue, color.Green / colorMaxValue, color.Blue / colorMaxValue, 1.0);
                    cr.Rectangle(Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                    cr.FillPreserve();
                }

                // Draw BorderColor
                if (_borderColor.HasValue)
                {
                    cr.LineWidth = _borderWidth;
                    var color = _borderColor.Value;
                    cr.SetSourceRGB(color.Red / colorMaxValue, color.Green / colorMaxValue, color.Blue / colorMaxValue);
                    cr.Rectangle(Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);
                    cr.StrokePreserve();
                }
            }

            return base.OnExposeEvent(evnt);
        }
    }
}