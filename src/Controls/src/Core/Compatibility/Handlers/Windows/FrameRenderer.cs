using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using WBorder = Microsoft.UI.Xaml.Controls.Border;
using WRect = Windows.Foundation.Rect;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class FrameRenderer : ViewRenderer<Frame, WBorder>
	{
		public static IPropertyMapper<Frame, FrameRenderer> Mapper
			= new PropertyMapper<Frame, FrameRenderer>(VisualElementRendererMapper);

		public static CommandMapper<Frame, FrameRenderer> CommandMapper
			= new CommandMapper<Frame, FrameRenderer>(VisualElementRendererCommandMapper);

		public FrameRenderer() : base(Mapper, CommandMapper)
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
					SetNativeControl(new WBorder());

				PackChild();
				UpdateBorder();
				UpdateCornerRadius();
				UpdatePadding();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "Content")
			{
				PackChild();
			}
			else if (e.PropertyName == Frame.BorderColorProperty.PropertyName || e.PropertyName == Frame.HasShadowProperty.PropertyName)
			{
				UpdateBorder();
			}
			else if (e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
			{
				UpdateCornerRadius();
			}
			else if (e.PropertyName == Frame.PaddingProperty.PropertyName)
			{
				UpdatePadding();
			}
		}

		void UpdatePadding()
		{
			Control.Padding = Element.Padding.ToPlatform();
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			Control.Arrange(new WRect(0, 0, finalSize.Width, finalSize.Height));
			if (Element is IContentView cv)
				cv.CrossPlatformArrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

			return finalSize;
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			var size = base.MeasureOverride(availableSize);

			if (Element is IContentView cv)
				size = cv.CrossPlatformMeasure(availableSize.Width, availableSize.Height).ToPlatform();

			return size;
		}

		protected override void UpdateBackgroundColor()
		{
			UpdateBackground();
		}

		protected override void UpdateBackground()
		{
			Color backgroundColor = Element.BackgroundColor;
			Brush background = Element.Background;

			if (Control != null)
			{
				if (Brush.IsNullOrEmpty(background))
					Control.Background = backgroundColor.IsDefault() ?
						new Microsoft.UI.Xaml.Media.SolidColorBrush((global::Windows.UI.Color)Resources["SystemAltHighColor"]) : backgroundColor.ToPlatform();
				else
					Control.Background = background.ToBrush();
			}
		}

		void PackChild()
		{
			if (Element.Content == null)
				return;

			Control.Child = Element.Content.ToPlatform(MauiContext);
		}

		void UpdateBorder()
		{
			if (Element.BorderColor.IsNotDefault())
			{
				Control.BorderBrush = Element.BorderColor.ToPlatform();
				Control.BorderThickness = WinUIHelpers.CreateThickness(1);
			}
			else
			{
				Control.BorderBrush = new Color(0, 0, 0, 0).ToPlatform();
			}
		}

		void UpdateCornerRadius()
		{
			float cornerRadius = Element.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // default corner radius

			Control.CornerRadius = WinUIHelpers.CreateCornerRadius(cornerRadius);
		}
	}
}