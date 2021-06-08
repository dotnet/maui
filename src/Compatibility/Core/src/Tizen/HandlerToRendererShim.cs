using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using EvasObject = ElmSharp.EvasObject;
using ERect = ElmSharp.Rect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class HandlerToRendererShim : IVisualElementRenderer
	{
		public HandlerToRendererShim(IViewHandler vh)
		{
			ViewHandler = vh;
		}

		IViewHandler ViewHandler { get; }

		public VisualElement Element { get; private set; }

		public EvasObject NativeView => ((INativeViewHandler)ViewHandler).NativeView;


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

			Platform.SetRenderer(element, this);

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, Element));

			(ViewHandler as INativeViewHandler)?.SetParent(new MockParentHandler(element.RealParent as VisualElement));
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
			IVisualElementRenderer Renderer => Platform.GetRenderer(RealParent);
			public EvasObject NativeView => Renderer.NativeView;

			public EvasObject ContainerView => NativeView;

			public INativeViewHandler Parent => null;

			public IView VirtualView => Renderer.Element as IView;

			public IMauiContext MauiContext => null;

			object IViewHandler.NativeView => NativeView;

			object IViewHandler.ContainerView => NativeView;

			bool HasContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			bool IViewHandler.HasContainer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public void DisconnectHandler() { }

			public void Dispose()
			{
				throw new NotImplementedException();
			}

			public Size GetDesiredSize(double widthConstraint, double heightConstraint)
			{
				throw new NotImplementedException();
			}

			public ERect GetNativeContentGeometry()
			{
				return Renderer.GetNativeContentGeometry();
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
		}

	}
}
