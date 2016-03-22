using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class LayoutRenderer : ViewRenderer<Layout, FrameworkElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				SizeChanged -= OnSizeChanged;
			}

			if (e.NewElement != null)
			{
				SizeChanged += OnSizeChanged;

				UpdateClipToBounds();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Layout.IsClippedToBoundsProperty.PropertyName)
				UpdateClipToBounds();
		}

		void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateClipToBounds();
		}

		void UpdateClipToBounds()
		{
			Clip = null;
			if (Element.IsClippedToBounds)
			{
				Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
			}
		}
	}
}