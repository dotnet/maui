using Tizen.UIExtensions.NUI;
using PlatformView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer : ViewRenderer<View, PlatformView>
	{
	}

	public abstract partial class ViewRenderer<TElement, TPlatformView> : VisualElementRenderer<TElement>, IPlatformViewHandler
		where TElement : View, IView
		where TPlatformView : PlatformView
	{
		TPlatformView? _platformView;
		ViewGroup? _container;

		public TPlatformView? Control => ((IElementHandler)this).PlatformView as TPlatformView ?? _platformView;
		object? IElementHandler.PlatformView => _platformView;

		public ViewRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{
		}

		protected ViewRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
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

		internal void SetNativeControl(TPlatformView control, ViewGroup container)
		{
			if (Control != null)
			{
				Children.Remove(Control);
			}

			_container = container;
			_platformView = control;

			var toAdd = container == this ? control : (PlatformView)container;
			toAdd.WidthSpecification = Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent;
			toAdd.HeightSpecification = Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent;
			Children.Add(toAdd);
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