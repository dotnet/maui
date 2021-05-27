using System;
#if __ANDROID__
using static Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat.Platform;
using NativeView = Android.Views.View;
using IVisualElementRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.IVisualElementRenderer;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, Android.Views.View>;
using VisualElementChangedEventArgs = Microsoft.Maui.Controls.Compatibility.Platform.Android.VisualElementChangedEventArgs;
#elif __IOS__ || MACCATALYST
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform;
using NativeView = UIKit.UIView;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, UIKit.UIView>;
#elif NETSTANDARD
using NativeView = System.Object;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, System.Object>;
#elif WINDOWS
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, Microsoft.UI.Xaml.FrameworkElement>;
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
using static Microsoft.Maui.Controls.Compatibility.Platform.UWP.Platform;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
#endif

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim : ViewHandler
	{
		public RendererToHandlerShim() : base(ViewHandler.ViewMapper)
		{
		}

#if __ANDROID__ || __IOS__ || WINDOWS
		internal IVisualElementRenderer VisualElementRenderer { get; private set; }

		public static IViewHandler CreateShim(object renderer)
		{
			if (renderer is IViewHandler handler)
				return handler;

			if (renderer is IVisualElementRenderer ivr)
				return new RendererToHandlerShim(ivr);

			return new RendererToHandlerShim();
		}

		public RendererToHandlerShim(IVisualElementRenderer visualElementRenderer) : this()
		{
			if (visualElementRenderer != null)
				SetupRenderer(visualElementRenderer);
		}

		public void SetupRenderer(IVisualElementRenderer visualElementRenderer)
		{
			VisualElementRenderer = visualElementRenderer;

			if (VisualElementRenderer.Element is IView view)
			{
				view.Handler = this;
				SetVirtualView(view);
			}
			else if (VisualElementRenderer.Element != null)
				throw new Exception($"{VisualElementRenderer.Element} must implement: {nameof(Microsoft.Maui.IView)}");

			VisualElementRenderer.ElementChanged += OnElementChanged;
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

		protected override void ConnectHandler(NativeView nativeView)
		{
			base.ConnectHandler(nativeView);
			VirtualView.Handler = this;
		}

		protected override void DisconnectHandler(NativeView nativeView)
		{
			SetRenderer(
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
				SetupRenderer(CreateRenderer(view));
			}

			SetRenderer(
				(VisualElement)view,
				VisualElementRenderer);

			if (VisualElementRenderer.Element != view)
			{
				VisualElementRenderer.SetElement((VisualElement)view);
			}
			else
			{
				base.SetVirtualView(view);
			}
		}
#else
		protected override NativeView CreateNativeView()
		{
			throw new NotImplementedException();
		}
#endif
	}
}
