#nullable enable
using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer : ViewRenderer<View, PlatformView>
	{
		protected ViewRenderer() : base()
		{
		}
	}

	public abstract partial class ViewRenderer<TElement, TNativeView> : VisualElementRenderer<TElement>, INativeViewHandler
		where TElement : View, IView
		where TNativeView : PlatformView
	{
		TNativeView? _nativeView;

		public TNativeView? Control => ((IElementHandler)this).NativeView as TNativeView ?? _nativeView;
		object? IElementHandler.NativeView => _nativeView;

		public ViewRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		internal ViewRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			var platformView = (this as IElementHandler).NativeView as UIView;
			if (platformView != null && Element != null)
			{
				platformView.Frame = new CoreGraphics.CGRect(0, 0, (nfloat)Element.Width, (nfloat)Element.Height);
			}
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);
		}

		public override void SizeToFit()
		{
			Control?.SizeToFit();
			base.SizeToFit();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			return Control?.SizeThatFits(size) ?? base.SizeThatFits(size);
		}

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

			if (Control != null)
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