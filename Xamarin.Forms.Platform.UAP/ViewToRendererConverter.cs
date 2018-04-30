using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
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

		internal class WrapperControl : Panel
		{
			readonly View _view;

			FrameworkElement FrameworkElement { get; }

			internal void CleanUp() => _view?.Cleanup();

			public WrapperControl(View view)
			{
				_view = view;
				_view.MeasureInvalidated += (sender, args) => { InvalidateMeasure(); };

				IVisualElementRenderer renderer = Platform.CreateRenderer(view);
				Platform.SetRenderer(view, renderer);

				FrameworkElement = renderer.ContainerElement;
				Children.Add(renderer.ContainerElement);

				// make sure we re-measure once the template is applied
				if (FrameworkElement != null)
				{
					FrameworkElement.Loaded += (sender, args) =>
					{
						// If the view is a layout (stacklayout, grid, etc) we need to trigger a layout pass
						// with all the controls in a consistent native state (i.e., loaded) so they'll actually
						// have Bounds set
						(_view as Layout)?.ForceLayout();
						InvalidateMeasure();
					};
				}
			}

			protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
			{
				_view.IsInNativeLayout = true;
				Layout.LayoutChildIntoBoundingRegion(_view, new Rectangle(0, 0, finalSize.Width, finalSize.Height));
				_view.IsInNativeLayout = false;

				FrameworkElement?.Arrange(new Rect(_view.X, _view.Y, _view.Width, _view.Height));
				return finalSize;
			}

			protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
			{
				Size request = _view.Measure(availableSize.Width, availableSize.Height, MeasureFlags.IncludeMargins).Request;

				if (request.Height < 0)
				{
					request.Height = availableSize.Height;
				}

				Windows.Foundation.Size result;
				if (_view.HorizontalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(availableSize.Width) && availableSize.Width != 0)
				{
					result = new Windows.Foundation.Size(availableSize.Width, request.Height);
				}
				else
				{
					result = new Windows.Foundation.Size(request.Width, request.Height);
				}

				_view.Layout(new Rectangle(0, 0, result.Width, result.Height));

				FrameworkElement?.Measure(availableSize);

				return result;
			}
		}
	}
}