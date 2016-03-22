using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using WSize = System.Windows.Size;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ViewRenderer<TElement, TNativeElement> : VisualElementRenderer<TElement, TNativeElement> where TElement : View where TNativeElement : FrameworkElement
	{
	}

	public class ViewRenderer : ViewRenderer<View, FrameworkElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged(e);
			SizeChanged += (sender, args) => UpdateClipToBounds();

			UpdateBackgroundColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Layout.IsClippedToBoundsProperty.PropertyName)
				UpdateClipToBounds();
		}

		protected override void UpdateNativeWidget()
		{
			base.UpdateNativeWidget();
			UpdateClipToBounds();
		}

		void UpdateClipToBounds()
		{
			var layout = Element as Layout;
			if (layout != null)
			{
				Clip = null;
				if (layout.IsClippedToBounds)
					Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
			}
		}
	}
}