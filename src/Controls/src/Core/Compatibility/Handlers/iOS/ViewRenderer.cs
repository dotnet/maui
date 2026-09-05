using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Graphics;
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

	public abstract partial class ViewRenderer<TElement, TPlatformView> : VisualElementRenderer<TElement>, IPlatformViewHandler, ISafeAreaScrollViewContainer
		where TElement : View, IView
		where TPlatformView : PlatformView
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The native view is cleared in DisconnectHandlerCore before the renderer disconnects.")]
		TPlatformView? _nativeView;
		Rect? _safeAreaFrame;

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
				platformView.Frame = _safeAreaFrame is null
					? new CoreGraphics.CGRect(0, 0, (nfloat)Element.Width, (nfloat)Element.Height)
					: Bounds;
			}
		}

		void ISafeAreaScrollViewContainer.ApplyDelegatedFrame(Rect safeFrame, Rect delegatedFrame)
		{
			if (Handle == IntPtr.Zero)
				return;

			_safeAreaFrame = safeFrame;
			Center = new CGPoint(delegatedFrame.Center.X, delegatedFrame.Center.Y);
			Bounds = new CGRect(Bounds.X, Bounds.Y, delegatedFrame.Width, delegatedFrame.Height);

			if ((this as IElementHandler).PlatformView is UIView platformView &&
				platformView != this &&
				platformView.Handle != IntPtr.Zero)
				platformView.Frame = Bounds;
		}

		void ISafeAreaScrollViewContainer.ResetDelegatedFrame()
		{
			if (_safeAreaFrame is null)
				return;

			_safeAreaFrame = null;
			if (Handle == IntPtr.Zero || Element is null)
				return;

			var safeFrame = Element.Frame;
			Center = new CGPoint(safeFrame.Center.X, safeFrame.Center.Y);
			Bounds = new CGRect(Bounds.X, Bounds.Y, safeFrame.Width, safeFrame.Height);

			if ((this as IElementHandler).PlatformView is UIView platformView &&
				platformView != this &&
				platformView.Handle != IntPtr.Zero)
				platformView.Frame = Bounds;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return
				new SizeRequest(this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint),
				MinimumSize());
		}
#pragma warning restore CS0618 // Type or member is obsolete

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
			_safeAreaFrame = null;

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