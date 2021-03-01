using System;
using System.ComponentModel;
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
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
			ViewHandler?.SetFrame(Element.Bounds);
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
	}
}
