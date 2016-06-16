using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ViewToRendererConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var view = value as View;
			if (view == null)
			{
				var page = value as Page;
				if (page != null)
				{
					IVisualElementRenderer renderer = page.GetOrCreateRenderer();
					return renderer.ContainerElement;
				}
			}

			if (view == null)
				return null;

			return new WrapperControl(view);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}

		class WrapperControl : ContentControl
		{
			readonly View _view;

			public WrapperControl(View view)
			{
				_view = view;
				_view.MeasureInvalidated += (sender, args) => { InvalidateMeasure(); };

				IVisualElementRenderer renderer = Platform.CreateRenderer(view);
				Platform.SetRenderer(view, renderer);

				NotifyWrapperAwareDescendants(view, renderer);

				Content = renderer.ContainerElement;

				// make sure we re-measure once the template is applied
				FrameworkElement frameworkElement = renderer.ContainerElement;
				if (frameworkElement != null)
					frameworkElement.Loaded += (sender, args) => InvalidateMeasure();
			}

			protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
			{
				_view.IsInNativeLayout = true;
				Layout.LayoutChildIntoBoundingRegion(_view, new Rectangle(0, 0, finalSize.Width, finalSize.Height));
				_view.IsInNativeLayout = false;

				var content = Content as FrameworkElement;
				content.Arrange(new Rect(_view.X, _view.Y, _view.Width, _view.Height));
				return finalSize;
			}

			protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
			{
				var content = Content as FrameworkElement;
				content?.Measure(availableSize);
				Size request = _view.Measure(availableSize.Width, availableSize.Height, MeasureFlags.IncludeMargins).Request;

				var result = new Windows.Foundation.Size();
				if (_view.HorizontalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(availableSize.Width) && availableSize.Width != 0)
				{
					result = new Windows.Foundation.Size(availableSize.Width, request.Height);
				}
				else
				{
					result = new Windows.Foundation.Size(request.Width, request.Height);
				}

				_view.Layout(new Rectangle(0, 0, result.Width, result.Width));
				return result;
			}

			void NotifyWrapperAwareDescendants(Element currentView, IVisualElementRenderer currentRenderer)
			{
				// If any of the child renderers need to handle anything differently because they're in 
				// a wrapper in a list view, let them know that they're being wrapped
				var wrapperAwareRenderer = currentRenderer as IWrapperAware;
				wrapperAwareRenderer?.NotifyWrapped();

				foreach (Element child in ((IElementController)currentView).LogicalChildren)
				{
					var childView = child as View;
					if (childView == null)
					{
						continue;
					}

					NotifyWrapperAwareDescendants(childView, Platform.GetRenderer(childView));
				}
			}
		}
	}
}