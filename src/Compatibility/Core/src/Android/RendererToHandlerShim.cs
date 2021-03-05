using System;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using AbstractViewHandler = Microsoft.Maui.Handlers.AbstractViewHandler<Microsoft.Maui.IView, Android.Views.View>;
using IVisualElementRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.IVisualElementRenderer;
using VisualElementChangedEventArgs = Microsoft.Maui.Controls.Compatibility.Platform.Android.VisualElementChangedEventArgs;

namespace Microsoft.Maui.Controls.Compatibility
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

		public RendererToHandlerShim() : base(Handlers.ViewHandler.ViewMapper)
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
				throw new Exception($"{VisualElementRenderer.Element} must implement: {nameof(Microsoft.Maui.IView)}");
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
				throw new Exception($"{e.NewElement} must implement: {nameof(Microsoft.Maui.IView)}");
		}

		protected override global::Android.Views.View CreateNativeView()
		{
			return VisualElementRenderer.View;
		}

		protected override void ConnectHandler(global::Android.Views.View nativeView)
		{
			base.ConnectHandler(nativeView);
			VirtualView.Handler = this;
		}

		protected override void DisconnectHandler(global::Android.Views.View nativeView)
		{
			Platform.Android.AppCompat.Platform.SetRenderer(
				VisualElementRenderer.Element,
				null);

			VisualElementRenderer.SetElement(null);

			base.DisconnectHandler(nativeView);
			VirtualView.Handler = null;
		}

		public override void SetVirtualView(IView view)
		{
			if (VisualElementRenderer == null && Context != null)
			{
				var renderer = Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view, Context)
										   ?? new Platform.Android.AppCompat.Platform.DefaultRenderer(Context);

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

			Platform.Android.AppCompat.Platform.SetRenderer(
				VisualElementRenderer.Element,
				VisualElementRenderer);
		}

		public override void UpdateValue(string property)
		{
			base.UpdateValue(property);
			if (property == "Frame")
			{
				SetFrame(VisualElementRenderer.Element.Bounds);
			}
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Platform.Android.AppCompat.Platform.GetNativeSize(
				VisualElementRenderer, widthConstraint, heightConstraint);
		}
	}
}
