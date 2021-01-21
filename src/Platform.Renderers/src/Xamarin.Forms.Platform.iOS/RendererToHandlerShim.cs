using System;
using Xamarin.Platform;
using IView = Xamarin.Platform.IView;
using AbstractViewHandler = Xamarin.Platform.Handlers.AbstractViewHandler<Xamarin.Platform.IView, UIKit.UIView>;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms
{
	public class RendererToHandlerShim : AbstractViewHandler
	{
		internal IVisualElementRenderer VisualElementRenderer { get; private set; }

		public static IViewHandler CreateShim(object renderer)
		{
			if (renderer is IViewHandler handler)
				return handler;

			if (renderer is IVisualElementRenderer ivr)
				return new RendererToHandlerShim(ivr);

			return new RendererToHandlerShim();
		}

		public RendererToHandlerShim() : base(Xamarin.Platform.Handlers.ViewHandler.ViewMapper)
		{
		}

		public RendererToHandlerShim(IVisualElementRenderer visualElementRenderer) : this()
		{
			if(visualElementRenderer != null)
				SetupRenderer(visualElementRenderer);
		}

		public void SetupRenderer(IVisualElementRenderer visualElementRenderer)
		{
			VisualElementRenderer = visualElementRenderer;
			VisualElementRenderer.ElementChanged += OnElementChanged;

			if (VisualElementRenderer.Element is IView view)
			{
				view.Handler = this;
				SetVirtualView(view);
			}
			else if (VisualElementRenderer.Element != null)
				throw new Exception($"{VisualElementRenderer.Element} must implement: {nameof(Xamarin.Platform.IView)}");
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement is IView view)
				view.Handler = null;

			if (e.NewElement is IView newView)
			{
				newView.Handler = this;				
				this.SetVirtualView(newView);
			}
			else if (e.NewElement != null)
				throw new Exception($"{e.NewElement} must implement: {nameof(Xamarin.Platform.IView)}");
		}

		protected override UIView CreateNativeView()
		{
			return VisualElementRenderer.NativeView;
		}

		protected override void ConnectHandler(UIView nativeView)
		{
			base.ConnectHandler(nativeView);
			VirtualView.Handler = this;
		}

		protected override void DisconnectHandler(UIView nativeView)
		{
			base.DisconnectHandler(nativeView);
			VirtualView.Handler = null;
		}

		public override void SetVirtualView(IView view)
		{
			if(VisualElementRenderer == null)
			{
				var renderer = Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view)
										   ?? new Xamarin.Forms.Platform.iOS.Platform.DefaultRenderer();

				SetupRenderer(renderer);
			}

			if (VisualElementRenderer.Element != view)
			{
				VisualElementRenderer.SetElement((VisualElement)view);
			}
			else
			{
				base.SetVirtualView(view);
			}
		}

		public void DisconnectHandler()
		{
			VisualElementRenderer.SetElement(null);
		}

		public override void UpdateValue(string property)
		{
			base.UpdateValue(property);
			if (property == "Frame")
			{
				SetFrame(VisualElementRenderer.Element.Bounds);
			}
		}
	}
}
