using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class FrameRenderer : ViewRenderer<Frame, Border>
	{
		public FrameRenderer()
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			SetNativeControl(new Border());

			PackChild();
			UpdateBorder();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "Content")
				PackChild();
			else if (e.PropertyName == Frame.OutlineColorProperty.PropertyName || e.PropertyName == Frame.HasShadowProperty.PropertyName)
				UpdateBorder();
		}

		void PackChild()
		{
			if (Element.Content == null)
				return;

			Platform.SetRenderer(Element.Content, Platform.CreateRenderer(Element.Content));

			UIElement element = Platform.GetRenderer(Element.Content).ContainerElement;
			Control.Child = element;
		}

		void UpdateBorder()
		{
			Control.CornerRadius = new CornerRadius(5);
			if (Element.OutlineColor != Color.Default)
			{
				Control.BorderBrush = Element.OutlineColor.ToBrush();
				Control.BorderThickness = new System.Windows.Thickness(1);
			}
			else
				Control.BorderBrush = new Color(0, 0, 0, 0).ToBrush();
		}
	}
}