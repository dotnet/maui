#nullable enable
using System;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer : ViewRenderer<View, PlatformView>
	{
		protected ViewRenderer(IMauiContext mauiContext) : base(mauiContext)
		{
		}
	}

	public abstract partial class ViewRenderer<TElement, TNativeView> : VisualElementRenderer<TElement>, INativeViewHandler
		where TElement : View, IView
		where TNativeView : PlatformView
	{
		TNativeView? _nativeView;

		public TNativeView? Control => ((IElementHandler)this).NativeView as TNativeView ?? _nativeView;

		protected virtual TNativeView CreateNativeControl()
		{
			return default(TNativeView)!;
		}

		protected void SetNativeControl(TNativeView control)
		{
			if (Control != null)
			{
				Control?.RemoveFromSuperview();
			}


			_nativeView = control;

			if(Control != null)
				AddSubview(Control);
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
	}
}