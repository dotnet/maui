using Gdk;
using Gtk;
using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;
using Xamarin.Forms.Platform.GTK.Helpers;
using NativeLabel = Gtk.Label;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class LabelRenderer : ViewRenderer<Label, NativeLabel>
    {
        private SizeRequest _perfectSize;
        private bool _perfectSizeValid;
        private bool _allocated;

        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (!_allocated && PlatformHelper.GetGTKPlatform() == GTKPlatform.Windows)
            {
                return default(SizeRequest);
            }

            if (!_perfectSizeValid)
            {
                _perfectSize = GetPerfectSize();
                _perfectSize.Minimum = new Size(Math.Min(10, _perfectSize.Request.Width), _perfectSize.Request.Height);
                _perfectSizeValid = true;
            }

            var widthFits = widthConstraint >= _perfectSize.Request.Width;
            var heightFits = heightConstraint >= _perfectSize.Request.Height;

            if (widthFits && heightFits)
                return _perfectSize;

            var result = GetPerfectSize((int)widthConstraint);
            var tinyWidth = Math.Min(10, result.Request.Width);
            result.Minimum = new Size(tinyWidth, result.Request.Height);

            if (widthFits || Element.LineBreakMode == LineBreakMode.NoWrap)
            {
                return new SizeRequest(
                    new Size(result.Request.Width, _perfectSize.Request.Height),
                    new Size(result.Minimum.Width, _perfectSize.Request.Height));
            }

            bool containerIsNotInfinitelyWide = !double.IsInfinity(widthConstraint);

            if (containerIsNotInfinitelyWide)
            {
                bool textCouldHaveWrapped = Element.LineBreakMode == LineBreakMode.WordWrap || Element.LineBreakMode == LineBreakMode.CharacterWrap;
                bool textExceedsContainer = result.Request.Width > widthConstraint;

                if (textExceedsContainer || textCouldHaveWrapped)
                {
                    var expandedWidth = Math.Max(tinyWidth, widthConstraint);
                    result.Request = new Size(expandedWidth, result.Request.Height);
                }
            }

            return result;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    SetNativeControl(new NativeLabel());
                }

                UpdateText();
                UpdateColor();
                UpdateLineBreakMode();
                UpdateTextAlignment();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Label.TextColorProperty.PropertyName)
                UpdateColor();
            else if (e.PropertyName == Label.FontProperty.PropertyName)
                UpdateText();
            else if (e.PropertyName == Label.TextProperty.PropertyName)
                UpdateText();
            else if (e.PropertyName == Label.FontAttributesProperty.PropertyName)
                UpdateText();
            else if (e.PropertyName == Label.FormattedTextProperty.PropertyName)
                UpdateText();
            else if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
                UpdateTextAlignment();
            else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
                UpdateLineBreakMode();
        }

        protected override void SetAccessibilityLabel()
        {
            var elemValue = (string)Element?.GetValue(AutomationProperties.NameProperty);

            if (string.IsNullOrWhiteSpace(elemValue) 
                && Control?.Accessible.Description == Control?.Text)
                return;

            base.SetAccessibilityLabel();
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            _allocated = true;

            Control.Layout.Width = Pango.Units.FromPixels((int)Element.Bounds.Width);
        }

        private void UpdateText()
        {
            _perfectSizeValid = false;

            string markupText = string.Empty;
            FormattedString formatted = Element.FormattedText;

            if (formatted != null)
            {
                Control.SetTextFromFormatted(formatted);
            }
            else
            {
                var span = new Span()
                {
                    FontAttributes = Element.FontAttributes,
                    FontFamily = Element.FontFamily,
                    FontSize = Element.FontSize,
                    Text = GLib.Markup.EscapeText(Element.Text ?? string.Empty)
                };

                Control.SetTextFromSpan(span);
            }
        }

        private void UpdateColor()
        {
            if (Control == null)
                return;

            var textColor = Element.TextColor != Color.Default ? Element.TextColor : Color.Black;

            Control.ModifyFg(StateType.Normal, textColor.ToGtkColor());
        }

        private void UpdateTextAlignment()
        {
            var hAlignmentValue = GetAlignmentValue(Element.HorizontalTextAlignment);
            var vAlignmentValue = GetAlignmentValue(Element.VerticalTextAlignment);

            Control.SetAlignment(hAlignmentValue, vAlignmentValue);
        }

        private void UpdateLineBreakMode()
        {
            _perfectSizeValid = false;

            switch (Element.LineBreakMode)
            {
                case LineBreakMode.NoWrap:
                    Control.LineWrap = false;
                    Control.Ellipsize = Pango.EllipsizeMode.None;
                    break;
                case LineBreakMode.WordWrap:
                    Control.LineWrap = true;
                    Control.LineWrapMode = Pango.WrapMode.Word;
                    Control.Ellipsize = Pango.EllipsizeMode.None;
                    break;
                case LineBreakMode.CharacterWrap:
                    Control.LineWrap = true;
                    Control.LineWrapMode = Pango.WrapMode.Char;
                    Control.Ellipsize = Pango.EllipsizeMode.None;
                    break;
                case LineBreakMode.HeadTruncation:
                    Control.LineWrap = false;
                    Control.LineWrapMode = Pango.WrapMode.Word;
                    Control.Ellipsize = Pango.EllipsizeMode.Start;
                    break;
                case LineBreakMode.TailTruncation:
                    Control.LineWrap = false;
                    Control.LineWrapMode = Pango.WrapMode.Word;
                    Control.Ellipsize = Pango.EllipsizeMode.End;
                    break;
                case LineBreakMode.MiddleTruncation:
                    Control.LineWrap = false;
                    Control.LineWrapMode = Pango.WrapMode.Word;
                    Control.Ellipsize = Pango.EllipsizeMode.Middle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float GetAlignmentValue(TextAlignment alignment)
        {
            _perfectSizeValid = false;

            switch (alignment)
            {
                case TextAlignment.Start:
                    return 0f;
                case TextAlignment.End:
                    return 1f;
                default:
                    return 0.5f;
            }
        }

        private SizeRequest GetPerfectSize(int widthConstraint = -1)
        {
            int w, h;
            Control.Layout.Width = Pango.Units.FromPixels(widthConstraint);
            Control.Layout.GetPixelSize(out w, out h);

            return new SizeRequest(new Size(w, h));
        }
    }
}