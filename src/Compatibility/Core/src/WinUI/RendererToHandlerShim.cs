using System;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, Microsoft.UI.Xaml.FrameworkElement>;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class RendererToHandlerShim : ViewHandler
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

		public RendererToHandlerShim() : base(ViewHandler.ViewMapper)
		{
		}

		public RendererToHandlerShim(IVisualElementRenderer visualElementRenderer) : this()
		{
			if (visualElementRenderer != null)
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
				throw new Exception($"{VisualElementRenderer.Element} must implement: {nameof(IView)}");
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
				throw new Exception($"{e.NewElement} must implement: {nameof(IView)}");
		}

		protected override FrameworkElement CreateNativeView()
		{
			return VisualElementRenderer.ContainerElement;
		}

		protected override void ConnectHandler(FrameworkElement nativeView)
		{
			base.ConnectHandler(nativeView);
			VirtualView.Handler = this;
		}

		protected override void DisconnectHandler(FrameworkElement nativeView)
		{
			Platform.UWP.Platform.SetRenderer(
				VisualElementRenderer.Element,
				null);

			VisualElementRenderer.SetElement(null);

			base.DisconnectHandler(nativeView);
			VirtualView.Handler = null;
		}

		public override void SetVirtualView(IView view)
		{
			if (VisualElementRenderer == null)
			{
				var renderer = Controls.Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view)
										   ?? new DefaultRenderer();

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

			Platform.UWP.Platform.SetRenderer(
				VisualElementRenderer.Element,
				VisualElementRenderer);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			return VisualElementRenderer.GetDesiredSize(widthConstraint, heightConstraint);
		}

	}
}
