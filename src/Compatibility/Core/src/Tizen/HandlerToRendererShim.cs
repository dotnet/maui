using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
#pragma warning disable CS0612 // Type or member is obsolete
	public class HandlerToRendererShim : IVisualElementRenderer
#pragma warning disable CS0612 // Type or member is obsolete
	{
		bool _disposed;

		public HandlerToRendererShim(IPlatformViewHandler vh)
		{
			Compatibility.Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
			ViewHandler = vh;
		}

		IPlatformViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public NView NativeView => ViewHandler.ContainerView ?? ViewHandler.PlatformView;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Platform.SetRenderer(Element, null);
				ViewHandler.Dispose();
			}
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
			Platform.SetRenderer(element, this);

			if (ViewHandler.VirtualView != element)
				ViewHandler.SetVirtualView((IView)element);

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Element));
		}

		void OnBatchCommitted(object sender, EventArg<VisualElement> e)
		{
			ViewHandler?.PlatformArrange(Element.Bounds);
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
		}

		void OnNativeDeleted(object sender, EventArgs e)
		{
			Dispose();
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return ViewHandler.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rect(Element.X, Element.Y, size.Width, size.Height));
		}

		public void UpdateLayout()
		{
			ViewHandler.PlatformArrange(Element.Bounds);
		}
	}
}
