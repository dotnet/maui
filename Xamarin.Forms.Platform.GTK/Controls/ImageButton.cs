using Gdk;
using Gtk;
using System;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public sealed class ImageButton : Gtk.Button
    {
        private Alignment _container;
        private Box _imageAndLabelContainer;

        private Gdk.Color _defaultBorderColor;
        private Gdk.Color _defaultBackgroundColor;
        private Gdk.Color? _borderColor;
        private Gdk.Color? _backgroundColor;

        private Gtk.Image _image;
        private Gtk.Label _label;
        private uint _imageSpacing = 0;
        private uint _borderWidth = 0;

        public ImageButton()
        {
            _defaultBackgroundColor = Style.Backgrounds[(int)StateType.Normal];
            _defaultBorderColor = Style.BaseColors[(int)StateType.Active];

            Relief = ReliefStyle.None;

            _image = new Gtk.Image();
            _label = new Gtk.Label();
            _container = new Alignment(0.5f, 0.5f, 0, 0);

            Add(_container);

            RecreateContainer();
        }

        public Gtk.Label LabelWidget => _label;

        public Gtk.Image ImageWidget => _image;

        public uint ImageSpacing
        {
            get
            {
                return _imageSpacing;
            }

            set
            {
                _imageSpacing = value;
                UpdateImageSpacing();
            }
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

        public void SetForegroundColor(Gdk.Color color)
        {
            _label.ModifyFg(StateType.Normal, color);
            _label.ModifyFg(StateType.Prelight, color);
            _label.ModifyFg(StateType.Active, color);
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

        public void SetImagePosition(PositionType position)
        {
            ImagePosition = position;
            RecreateContainer();
        }

        public void SetImageFromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            try
            {
                var iconPixBuf = new Pixbuf(fileName);

                if (iconPixBuf != null)
                {
                    ImageWidget.Pixbuf = iconPixBuf;
                }
            }
            catch (Exception ex)
            {
                Internals.Log.Warning("Image Loading", $"Image failed to load: {ex}");
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _label = null;
            _image = null;
            _imageAndLabelContainer = null;
            _container = null;
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            double colorMaxValue = 65535;

            using (var cr = CairoHelper.Create(GdkWindow))
            {
                cr.Rectangle(Allocation.Left, Allocation.Top, Allocation.Width, Allocation.Height);

                // Draw BackgroundColor
                if (_backgroundColor.HasValue)
                {
                    var color = _backgroundColor.Value;
                    cr.SetSourceRGBA(color.Red / colorMaxValue, color.Green / colorMaxValue, color.Blue / colorMaxValue, 1.0);
                    cr.FillPreserve();
                }

                // Draw BorderColor
                if (_borderColor.HasValue)
                {
                    cr.LineWidth = _borderWidth;

                    var color = _borderColor.Value;
                    cr.SetSourceRGB(color.Red / colorMaxValue, color.Green / colorMaxValue, color.Blue / colorMaxValue);
                    cr.Stroke();
                }
            }

            return base.OnExposeEvent(evnt);
        }

        private void RecreateContainer()
        {
            if (_imageAndLabelContainer != null)
            {
                _imageAndLabelContainer.RemoveFromContainer(_image);
                _imageAndLabelContainer.RemoveFromContainer(_label);
                _container.RemoveFromContainer(_imageAndLabelContainer);
                _imageAndLabelContainer = null;
            }

            switch (ImagePosition)
            {
                case PositionType.Left:
                    _imageAndLabelContainer = new HBox();
                    _imageAndLabelContainer.PackStart(_image, false, false, _imageSpacing);
                    _imageAndLabelContainer.PackStart(_label, false, false, 0);
                    break;
                case PositionType.Top:
                    _imageAndLabelContainer = new VBox();
                    _imageAndLabelContainer.PackStart(_image, false, false, _imageSpacing);
                    _imageAndLabelContainer.PackStart(_label, false, false, 0);
                    break;
                case PositionType.Right:
                    _imageAndLabelContainer = new HBox();
                    _imageAndLabelContainer.PackStart(_label, false, false, 0);
                    _imageAndLabelContainer.PackStart(_image, false, false, _imageSpacing);
                    break;
                case PositionType.Bottom:
                    _imageAndLabelContainer = new VBox();
                    _imageAndLabelContainer.PackStart(_label, false, false, 0);
                    _imageAndLabelContainer.PackStart(_image, false, false, _imageSpacing);
                    break;
            }

            if (_imageAndLabelContainer != null)
            {
                _container.Add(_imageAndLabelContainer);
                _container.ShowAll();
            }
        }

        private void UpdateImageSpacing()
        {
            _imageAndLabelContainer.SetChildPacking(_image, false, false, _imageSpacing, PackType.Start);
        }
    }
}
