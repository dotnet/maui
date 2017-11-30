using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Xamarin.Forms.Platform.UWP
{
	internal sealed class BackgroundTracker<T> : VisualElementTracker<Page, T> where T : FrameworkElement
	{
		readonly DependencyProperty _backgroundProperty;
		bool _backgroundNeedsUpdate = true;

		public BackgroundTracker(DependencyProperty backgroundProperty)
		{
			if (backgroundProperty == null)
				throw new ArgumentNullException("backgroundProperty");

			_backgroundProperty = backgroundProperty;
		}

		protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == Page.BackgroundImageProperty.PropertyName)
			{
				UpdateBackground();
			}

			base.OnPropertyChanged(sender, e);
		}

		protected override void UpdateNativeControl()
		{
			base.UpdateNativeControl();

			if (_backgroundNeedsUpdate)
				UpdateBackground();
		}

		void UpdateBackground()
		{
			if (Element == null)
				return;

			FrameworkElement element = Control ?? Container;
			if (element == null)
				return;

			string backgroundImage = Element.BackgroundImage;
			if (backgroundImage != null)
			{
				Uri uri;
				if (!Uri.TryCreate(backgroundImage, UriKind.RelativeOrAbsolute, out uri) || !uri.IsAbsoluteUri)
					uri = new Uri("ms-appx:///" + backgroundImage);

				element.SetValue(_backgroundProperty, new ImageBrush { ImageSource = new BitmapImage(uri) });
			}
			else
			{
				Color backgroundColor = Element.BackgroundColor;
				if (!backgroundColor.IsDefault)
				{
					element.SetValue(_backgroundProperty, backgroundColor.ToBrush());
				}
				else
				{
					object localBackground = element.ReadLocalValue(_backgroundProperty);
					if (localBackground != null && localBackground != DependencyProperty.UnsetValue)
						element.ClearValue(_backgroundProperty);
				}
			}

			_backgroundNeedsUpdate = false;
		}
	}
}