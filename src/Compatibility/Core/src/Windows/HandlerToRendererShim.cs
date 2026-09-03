using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		bool _nativeViewLoaded;

		public HandlerToRendererShim(IPlatformViewHandler vh)
		{
			Compatibility.Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
			ViewHandler = vh;
		}

		IPlatformViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public FrameworkElement ContainerElement => ViewHandler.ContainerView ?? ViewHandler.PlatformView;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void Dispose()
		{
			ViewHandler.DisconnectHandler();
		}

		public void SetElement(VisualElement element)
		{
			if (element == Element)
				return;

			var oldElement = Element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (element != null)
			{
				element.PropertyChanged += OnElementPropertyChanged;
			}

			Element = element;

			((IView)element).Handler = ViewHandler;

			if (ViewHandler.VirtualView != element)
			{
				ViewHandler.SetVirtualView((IView)element);
			}

			if (ViewHandler.PlatformView is FrameworkElement frameworkElement)
			{
				frameworkElement.Loaded += NativeViewLoaded;
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Element));
		}

		void NativeViewLoaded(object sender, RoutedEventArgs e)
		{
			_nativeViewLoaded = true;
			((FrameworkElement)sender).Loaded -= NativeViewLoaded;

			// For old-school renderers on Windows, VisualElementRenderer watches for the Loaded event and 
			// sets IsPlatformStateConsistent, which invalidates the measure for the element. This tells everything 
			// to lay out again because all the layout activity previous to that was invalid - the state of the control
			// was undefined with regard to measurement, and internal stuff like DesiredSize might be wrong or reset
			// at this point. Since we're using a shim to imitate a renderer, we're dealing with legacy layouts. Which
			// means we have to deal with the fact that they've started doing measure/layout even though it's not
			// time for that yet. So we need to imitate what VisualElementRenderer does and update IsPlatformStateConsistent
			// and force a re-layout with new measurements.

			if (Element is VisualElement visualElement)
			{
				visualElement.IsPlatformStateConsistent = true;

				if (visualElement is Layout layout)
				{
					// Unfortunately, the layout and its children will have cached their previous measurement results
					// So we need to iterate over the children and force them to clear their caches so they'll call
					// the native measurement methods again now that measurement is a valid thing to do.
					foreach (var child in layout.InternalChildren)
					{
						if (child is VisualElement ve)
						{
							ve.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
						}
					}

					layout.ForceLayout();
				}
			}
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (!_nativeViewLoaded)
			{
				return new SizeRequest(Size.Zero);
			}

			var size = ViewHandler.GetDesiredSize(widthConstraint, heightConstraint);
			return new SizeRequest(size, size);
		}

		public UIElement GetNativeElement()
		{
			return (FrameworkElement)ViewHandler.PlatformView;
		}
	}
}
