using System;
using System.ComponentModel;
using UIKit;
using Xamarin.Platform;

namespace Xamarin.Forms.Platform.iOS
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		public HandlerToRendererShim(IViewHandler vh)
		{
			ViewHandler = vh;
		}

		IViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public UIView NativeView => (UIView)ViewHandler.NativeView;

		public UIViewController ViewController => null;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void Dispose()
		{
			ViewHandler.DisconnectHandler();
		}

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			if(oldElement != null)
				oldElement.PropertyChanged -= OnElementPropertyChanged;

			if (Element != null)
				Element.PropertyChanged += OnElementPropertyChanged;

			Element = element;
			ViewHandler.SetVirtualView((IView)element);
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Element));
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public void SetElementSize(Size size)
		{
			Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}
	}
}
