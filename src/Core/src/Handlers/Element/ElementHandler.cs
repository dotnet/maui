using System;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ElementHandler : IElementHandler
	{
		public static IPropertyMapper<IElement, ElementHandler> ElementMapper = new PropertyMapper<IElement, ElementHandler>()
		{
		};

		protected IPropertyMapper _mapper;
		protected CommandMapper? CommandMapper;
		protected readonly IPropertyMapper _defaultMapper;

		protected ElementHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
			CommandMapper = commandMapper;
		}

		public IMauiContext? MauiContext { get; private set; }

		public IServiceProvider? Services => MauiContext?.Services;

		public object? NativeView { get; private protected set; }

		public IElement? VirtualView { get; private protected set; }

		public void SetMauiContext(IMauiContext mauiContext) =>
			MauiContext = mauiContext;

		public virtual void SetVirtualView(IElement view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (VirtualView == view)
				return;

			var oldVirtualView = VirtualView;
			if (oldVirtualView?.Handler != null)
				oldVirtualView.Handler = null;

			bool setupNativeView = oldVirtualView == null;

			VirtualView = view;
			NativeView ??= CreateNativeElement();

			if (VirtualView.Handler != this)
				VirtualView.Handler = this;

			if (setupNativeView)
			{
				ConnectHandler(NativeView);
			}

			_mapper = _defaultMapper;

			if (VirtualView is IPropertyMapperView imv)
			{
				var map = imv.GetPropertyMapperOverrides();
				if (map is not null)
				{
					map.Chained = _defaultMapper;
					_mapper = map;
				}
			}

			_mapper.UpdateProperties(this, VirtualView);
		}

		public virtual void UpdateValue(string property)
		{
			if (VirtualView == null)
				return;

			_mapper?.UpdateProperty(this, VirtualView, property);
		}

		public virtual void Invoke(string command, object? args)
		{
			if (VirtualView == null)
				return;

			CommandMapper?.Invoke(this, VirtualView, command, args);
		}

		private protected abstract object OnCreateNativeElement();

		object CreateNativeElement() =>
			OnCreateNativeElement();

		private protected abstract void OnConnectHandler(object nativeView);

		void ConnectHandler(object nativeView) =>
			OnConnectHandler(nativeView);

		private protected abstract void OnDisconnectHandler(object nativeView);

		void DisconnectHandler(object nativeView)
		{
			OnDisconnectHandler(nativeView);

			// VirtualView has already been changed over to a new handler
			if (VirtualView != null && VirtualView.Handler == this)
				VirtualView.Handler = null;

			VirtualView = null;
		}

		void IElementHandler.DisconnectHandler()
		{
			if (NativeView != null && VirtualView != null)
			{
				// We set the NativeView to null so no one outside of this handler tries to access
				// NativeView. NativeView access should be isolated to the instance passed into
				// DisconnectHandler
				var oldNativeView = NativeView;
				NativeView = null;
				DisconnectHandler(oldNativeView);
			}
		}
	}
}
