using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ViewToRendererConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var view = value as View;
			if (view == null)
				return null;

			return new WrapperControl(view);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		class WrapperControl : ContentControl
		{
			readonly View _view;

			public WrapperControl(View view)
			{
				_view = view;
				_view.MeasureInvalidated += (sender, args) => InvalidateMeasure();

				IVisualElementRenderer visualElementRenderer = Platform.CreateRenderer(view);
				Platform.SetRenderer(view, visualElementRenderer);
				Content = visualElementRenderer.ContainerElement;

				// make sure we re-measure once the template is applied
				var frameworkElement = visualElementRenderer.ContainerElement as FrameworkElement;
				if (frameworkElement != null)
				{
					frameworkElement.Loaded += (sender, args) =>
					{
						(_view as Layout)?.ForceLayout();
						((IVisualElementController)_view).InvalidateMeasure(InvalidationTrigger.MeasureChanged);
						InvalidateMeasure();
					};
				}
			}

			protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
			{
				_view.IsInNativeLayout = true;
				Layout.LayoutChildIntoBoundingRegion(_view, new Rectangle(0, 0, finalSize.Width, finalSize.Height));
				_view.IsInNativeLayout = false;

				var content = Content as FrameworkElement;
				content?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
				return finalSize;
			}

			protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
			{
				var content = Content as FrameworkElement;
				Size request = _view.Measure(availableSize.Width, availableSize.Height, MeasureFlags.IncludeMargins).Request;

				System.Windows.Size result;
				if (_view.HorizontalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(availableSize.Width) && availableSize.Width != 0)
				{
					result = new System.Windows.Size(availableSize.Width, request.Height);
				}
				else
				{
					result = new System.Windows.Size(request.Width, request.Height);
				}

				_view.Layout(new Rectangle(0, 0, result.Width, result.Height));

				content?.Measure(availableSize);

				return result;
			}
		}
	}
}