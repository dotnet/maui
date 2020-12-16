using CoreGraphics;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS.CustomRenderers;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Issue12372Button), typeof(_12372CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.iOS.CustomRenderers
{
	public class _12372CustomRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> args)
        {
            base.OnElementChanged(args);

            if (Control != null && Element != null)
            {
                SetColors();
                Control.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                Control.TitleLabel.TextAlignment = UITextAlignment.Center;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (Control == null || Element == null || args == null)
                return;

            if (args.PropertyName == nameof(Button.IsEnabled) ||
                args.PropertyName == nameof(Button.IsPressed) ||
                args.PropertyName == nameof(Button.IsVisible))
            {
                SetColors();
            }
        }

        private void SetColors()
        {
            if (Element is Issue12372Button nymblButton)
            {
                Control.SetTitleColor(nymblButton.NymblTextColor.ToUIColor(), UIControlState.Normal);
                Control.SetTitleColor(nymblButton.NymblPressedColor.ToUIColor(), UIControlState.Selected);
                Control.SetTitleColor(nymblButton.NymblDisabledTextColor.ToUIColor(), UIControlState.Disabled);

                if (nymblButton.IsEnabled && !nymblButton.IsPressed)
                {
                    Control.BackgroundColor = nymblButton.NymblDefaultColor.ToUIColor();
                    CreateShadow();
                }
                else if (nymblButton.IsEnabled && nymblButton.IsPressed)
                {
                    Control.BackgroundColor = nymblButton.NymblPressedColor.ToUIColor();
                    RemoveShadow();
                }
                else if (!nymblButton.IsEnabled)
                {
                    Control.BackgroundColor = nymblButton.NymblDisabledColor.ToUIColor();
                    RemoveShadow();
                }
                else
                {
                    // Any other state?
                    Control.BackgroundColor = nymblButton.NymblDefaultColor.ToUIColor();
                    RemoveShadow();
                }
            }
        }

        private void CreateShadow()
        {
            Layer.CornerRadius = 20;
            Layer.ShadowRadius = 2;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowOffset = new CGSize(4, 4);
            Layer.ShadowOpacity = 0.80f;
        }

        private void RemoveShadow()
        {
            Layer.CornerRadius = 20;
            Layer.ShadowOpacity = 0f;
        }
    }
}

