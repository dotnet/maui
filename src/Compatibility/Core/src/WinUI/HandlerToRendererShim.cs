using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		public HandlerToRendererShim(IViewHandler vh)
		{
			ViewHandler = vh;
		}

		IViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public FrameworkElement NativeView => (FrameworkElement)ViewHandler.NativeView;

		public FrameworkElement ContainerElement => (FrameworkElement)ViewHandler.NativeView;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void Dispose()
		{
			ViewHandler.DisconnectHandler();
		}

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				oldElement.BatchCommitted -= OnBatchCommitted;
			}

			if (element != null)
			{
				element.PropertyChanged += OnElementPropertyChanged;
				element.BatchCommitted += OnBatchCommitted;
			}

			Element = element;
			ViewHandler.SetVirtualView((IView)element);
			((IView)element).Handler = ViewHandler;

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Element));
		}

		void OnBatchCommitted(object sender, EventArg<VisualElement> e)
		{
			ViewHandler?.NativeArrange(Element.Bounds);
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = ViewHandler.GetDesiredSize(widthConstraint, heightConstraint);
			return new SizeRequest(size, size);
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public UIElement GetNativeElement()
		{
			return (FrameworkElement)ViewHandler.NativeView;
		}
	}
}
