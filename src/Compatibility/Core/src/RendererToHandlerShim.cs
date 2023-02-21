#nullable enable

using System;
using Microsoft.Maui.Controls.Platform;
#if __ANDROID__
#pragma warning disable CS0612 // Type or member is obsolete
using static Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform;
#pragma warning restore CS0612 // Type or member is obsolete
using PlatformView = Android.Views.View;
using IVisualElementRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.IVisualElementRenderer;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, Android.Views.View>;
#elif __IOS__ || MACCATALYST
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform;
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
using PlatformView = UIKit.UIView;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, UIKit.UIView>;
#elif TIZEN
#pragma warning disable CS0612 // Type or member is obsolete
using static Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Platform;
using PlatformView = Tizen.NUI.BaseComponents.View;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, Tizen.NUI.BaseComponents.View>;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, System.Object>;
#elif WINDOWS
using ViewHandler = Microsoft.Maui.Handlers.ViewHandler<Microsoft.Maui.IView, Microsoft.UI.Xaml.FrameworkElement>;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#pragma warning disable CS0612 // Type or member is obsolete
using static Microsoft.Maui.Controls.Compatibility.Platform.UWP.Platform;
#pragma warning restore CS0612 // Type or member is obsolete
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
#endif

namespace Microsoft.Maui.Controls.Compatibility
{
	[Obsolete]
	public partial class RendererToHandlerShim : ViewHandler
	{
		public static PropertyMapper<IView, ViewHandler> ShimMapper = new PropertyMapper<IView, ViewHandler>(ViewHandler.ViewMapper)
		{
			// These properties are already being handled by the shimmed renderer
			[nameof(IView.Background)] = MapIgnore,
			[nameof(IView.IsEnabled)] = MapIgnore,
			[nameof(IView.Opacity)] = MapIgnore,
			[nameof(IView.TranslationX)] = MapIgnore,
			[nameof(IView.TranslationY)] = MapIgnore,
			[nameof(IView.Scale)] = MapIgnore,
			[nameof(IView.ScaleX)] = MapIgnore,
			[nameof(IView.ScaleY)] = MapIgnore,
			[nameof(IView.Rotation)] = MapIgnore,
			[nameof(IView.RotationX)] = MapIgnore,
			[nameof(IView.RotationY)] = MapIgnore,
			[nameof(IView.AnchorX)] = MapIgnore,
			[nameof(IView.AnchorY)] = MapIgnore
		};

		static void MapIgnore(ViewHandler arg1, IView arg2)
		{
			// These are properties that are already being handled by the shimmed renderer
			// So if we also process these properties on the ViewHandler then we might get competing results
		}

		public RendererToHandlerShim() : base(ShimMapper)
		{
			Hosting.MauiAppBuilderExtensions.CheckForCompatibility();
		}

#if PLATFORM
		internal IVisualElementRenderer? VisualElementRenderer { get; private set; }
		new IView? VirtualView => (this as IViewHandler).VirtualView;

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

				if (VirtualView != view)
					SetVirtualView(view);
			}
			else if (VisualElementRenderer.Element != null)
				throw new Exception($"{VisualElementRenderer.Element} must implement: {nameof(Microsoft.Maui.IView)}");

			VisualElementRenderer.ElementChanged += OnElementChanged;
		}

		void OnElementChanged(object? sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement is IView view)
				view.Handler = null;

			if (e.NewElement is IView newView)
			{
				var currentContext = newView.Handler?.MauiContext;
				newView.Handler = this;
				if (this.MauiContext == null && currentContext != null)
				{
					this.SetMauiContext(currentContext);
				}

				if (VirtualView != newView)
					this.SetVirtualView(newView);
			}
			else if (e.NewElement != null)
				throw new Exception($"{e.NewElement} must implement: {nameof(Microsoft.Maui.IView)}");
		}

		protected override void ConnectHandler(PlatformView platformView)
		{
			base.ConnectHandler(platformView);
			base.VirtualView.Handler = this;
		}

		protected override void DisconnectHandler(PlatformView platformView)
		{
			VisualElementRenderer?.Dispose();
			base.DisconnectHandler(platformView);
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

			if (VisualElementRenderer != null && VisualElementRenderer.Element != view)
			{
				VisualElementRenderer.SetElement((VisualElement)view);
			}

			if (view != VirtualView)
				base.SetVirtualView(view);
		}
#else
		protected override PlatformView CreatePlatformView()
		{
			throw new NotImplementedException();
		}
#endif
	}
}
