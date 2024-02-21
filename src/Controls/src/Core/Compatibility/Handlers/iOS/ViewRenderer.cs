using CoreGraphics;
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

	public abstract partial class ViewRenderer<TElement, TPlatformView> : VisualElementRenderer<TElement>, IPlatformViewHandler
		where TElement : View, IView
		where TPlatformView : PlatformView
	{
		TPlatformView? _nativeView;

		public TPlatformView? Control
		{
			get
			{
				var value = ((IElementHandler)this).PlatformView as TPlatformView;
				if (value != this && value != null)
					return value;

				return _nativeView;
			}
		}

		object? IElementHandler.PlatformView => (_nativeView as object) ?? this;

		public ViewRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		protected ViewRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			var platformView = (this as IElementHandler).PlatformView as UIView;
			if (platformView != null && Element != null)
			{
				platformView.Frame = new CoreGraphics.CGRect(0, 0, (nfloat)Element.Width, (nfloat)Element.Height);
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return
				new SizeRequest(this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint),
				MinimumSize());
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

		protected virtual TPlatformView CreateNativeControl()
		{
			return default(TPlatformView)!;
		}

		protected void SetNativeControl(TPlatformView control)
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

		protected virtual void DisconnectHandler(TPlatformView oldNativeView)
		{
		}
	}
}