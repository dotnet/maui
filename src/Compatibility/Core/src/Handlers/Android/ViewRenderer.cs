#nullable enable
using System;
using PlatformView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract class ViewRenderer : ViewRenderer<View, PlatformView>
	{
		protected ViewRenderer(IMauiContext mauiContext) : base(mauiContext)
		{
		}
	}

	public abstract partial class ViewRenderer<TElement, TNativeView> : VisualElementRenderer<TElement>, INativeViewHandler
		where TElement : Element, IView
		where TNativeView : PlatformView
	{
		TNativeView? _nativeView;
		AViewGroup? _container;

		public TNativeView? Control => ((IElementHandler)this).NativeView as TNativeView ?? _nativeView;



		public ViewRenderer(IMauiContext mauiContext) : this(mauiContext, VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		internal ViewRenderer(IMauiContext context, IPropertyMapper mapper, CommandMapper? commandMapper = null) 
			: base(context, mapper, commandMapper)
		{
		}

		protected virtual TNativeView CreateNativeControl()
		{
			return default(TNativeView)!;
		}

		protected void SetNativeControl(TNativeView control)
		{
			SetNativeControl(control, this);
		}

		internal void SetNativeControl(TNativeView control, AViewGroup container)
		{
			if (Control != null)
			{
				RemoveView(Control);
			}

			_container = container;
			_nativeView = control;

			var toAdd = container == this ? control : (PlatformView)container;
			AddView(toAdd, LayoutParams.MatchParent);
		}

		private protected override void DisconnectHandlerCore()
		{
			if (_nativeView != null && Element != null)
			{
				// We set the NativeView to null so no one outside of this handler tries to access
				// NativeView. NativeView access should be isolated to the instance passed into
				// DisconnectHandler
				var oldNativeView = _nativeView;
				_nativeView = null;
				DisconnectHandler(oldNativeView);
			}

			base.DisconnectHandlerCore();
		}

		protected virtual void DisconnectHandler(TNativeView oldNativeView)
		{
		}

		PlatformView? INativeViewHandler.ContainerView => _container;
	}
}