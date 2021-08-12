using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ERect = ElmSharp.Rect;
using EvasObject = ElmSharp.EvasObject;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		public HandlerToRendererShim(INativeViewHandler vh)
		{
			ViewHandler = vh;
		}

		INativeViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public EvasObject NativeView => ViewHandler.ContainerView ?? ViewHandler.NativeView;


		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public void Dispose()
		{
			ViewHandler.Dispose();
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

			if (element.RealParent is IView view && view.Handler is INativeViewHandler nvh)
			{
				ViewHandler.SetParent(nvh);
			}
			else
			{
				ViewHandler.SetParent(new MockParentHandler(element.RealParent as VisualElement));
			}

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
			return ViewHandler.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public void SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public void UpdateLayout()
		{
			ViewHandler.NativeArrange(Element.Bounds);
		}

		public ERect GetNativeContentGeometry()
		{
			return NativeView.Geometry;
		}

		class MockParentHandler : INativeViewHandler
		{

			public MockParentHandler(VisualElement parent)
			{
				RealParent = parent;
			}

			VisualElement RealParent { get; }
			IVisualElementRenderer Renderer => RealParent != null ? Platform.GetRenderer(RealParent) : null;
			public EvasObject NativeView => Renderer.NativeView;

			public EvasObject ContainerView => NativeView;

			public INativeViewHandler Parent => null;

			public IView VirtualView => Renderer.Element as IView;

			public IMauiContext MauiContext => null;

			object IElementHandler.NativeView => NativeView;

			object IViewHandler.ContainerView => NativeView;

			bool IViewHandler.HasContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			Maui.IElement IElementHandler.VirtualView => throw new NotImplementedException();

			public void DisconnectHandler() { }

			public void Dispose()
			{
			}

			public Size GetDesiredSize(double widthConstraint, double heightConstraint)
			{
				throw new NotImplementedException();
			}

			public ERect GetNativeContentGeometry()
			{
				return Renderer?.GetNativeContentGeometry() ?? new ERect(0, 0, 0, 0);
			}

			public void NativeArrange(Rectangle frame)
			{
				throw new NotImplementedException();
			}

			public void SetMauiContext(IMauiContext mauiContext)
			{
				throw new NotImplementedException();
			}

			public void SetParent(INativeViewHandler parent)
			{
				throw new NotImplementedException();
			}

			public void SetVirtualView(IView view)
			{
				throw new NotImplementedException();
			}

			public void UpdateValue(string property)
			{
				throw new NotImplementedException();
			}

			public void SetVirtualView(Maui.IElement view)
			{
				throw new NotImplementedException();
			}

			public void Invoke(string command, object args)
			{
				throw new NotImplementedException();
			}
		}

	}
}
