using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		public HandlerToRendererShim(IPlatformViewHandler vh)
		{
			ViewHandler = vh;
		}

		IPlatformViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public UIView NativeView => ViewHandler.ContainerView ?? ViewHandler.PlatformView;

		public UIViewController ViewController => ViewHandler.ViewController;

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
			((IView)element).Handler = ViewHandler;

			if (ViewHandler.VirtualView != element)
				ViewHandler.SetVirtualView((IView)element);

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Element));
		}

		// TODO ezhart 2021-06-18 Review this; a control calling Arrange on itself is almost certainly wrong, but removing this right now is breaking
		// any layout that's inside a shimmed ScrollView. 
		void OnBatchCommitted(object sender, EventArg<VisualElement> e)
		{
			ViewHandler?.PlatformArrange(Element.Bounds);
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
			Layout.LayoutChildIntoBoundingRegion(Element, new Rect(Element.X, Element.Y, size.Width, size.Height));
		}
	}
}
