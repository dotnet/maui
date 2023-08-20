using System;
using Android.Content;
using AViewGroup = Android.Views.ViewGroup;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer : ViewRenderer<View, PlatformView>
	{
		protected ViewRenderer(Context context) : base(context)
		{
		}
	}

	public abstract partial class ViewRenderer<TElement, TPlatformView> : VisualElementRenderer<TElement>, IPlatformViewHandler
		where TElement : View, IView
		where TPlatformView : PlatformView
	{
		TPlatformView? _platformView;
		AViewGroup? _container;

		public TPlatformView? Control
		{
			get
			{
				var value = ((IElementHandler)this).PlatformView as TPlatformView;
				if (value != this && value != null)
					return value;

				return _platformView;
			}
		}

		object? IElementHandler.PlatformView => (_platformView as object) ?? this;

		public ViewRenderer(Context context) : this(context, VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		protected ViewRenderer(Context context, IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(context, mapper, commandMapper)
		{
		}

		protected virtual TPlatformView CreateNativeControl()
		{
			return default(TPlatformView)!;
		}

		protected void SetNativeControl(TPlatformView control)
		{
			SetNativeControl(control, this);
		}

		internal void SetNativeControl(TPlatformView control, AViewGroup container)
		{
			if (Control != null)
			{
				RemoveView(Control);
			}

			_container = container;
			_platformView = control;

			var toAdd = container == this ? control : (PlatformView)container;
			AddView(toAdd, LayoutParams.MatchParent);
		}

		private protected override void DisconnectHandlerCore()
		{
			if (_platformView != null && Element != null)
			{
				// We set the PlatformView to null so no one outside of this handler tries to access
				// PlatformView. PlatformView access should be isolated to the instance passed into
				// DisconnectHandler
				var oldPlatformView = _platformView;
				_platformView = null;
				DisconnectHandler(oldPlatformView);
			}

			base.DisconnectHandlerCore();
		}

		protected virtual void DisconnectHandler(TPlatformView oldPlatformView)
		{
		}

		PlatformView? IPlatformViewHandler.ContainerView => _container;
	}
}