using System;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ElementHandler : IElementHandler, IElementHandlerStateExhibitor
	{
		public static IPropertyMapper<IElement, IElementHandler> ElementMapper = new PropertyMapper<IElement, IElementHandler>()
		{
		};

		public static CommandMapper<IElement, IElementHandler> ElementCommandMapper = new CommandMapper<IElement, IElementHandler>()
		{
		};

		internal readonly IPropertyMapper _defaultMapper;
		internal readonly CommandMapper? _commandMapper;
		internal IPropertyMapper _mapper;
		ElementHandlerState _handlerState;

		ElementHandlerState IElementHandlerStateExhibitor.State => _handlerState;

		protected ElementHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
			_commandMapper = commandMapper;
		}

		public IMauiContext? MauiContext { get; private set; }

		public IServiceProvider? Services => MauiContext?.Services;

		public object? PlatformView { get; private protected set; }

		public IElement? VirtualView { get; private protected set; }

		public virtual void SetMauiContext(IMauiContext mauiContext) =>
			MauiContext = mauiContext;

		public virtual void SetVirtualView(IElement view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (VirtualView == view)
			{
				return;
			}

			var oldVirtualView = VirtualView;

			bool setupPlatformView = oldVirtualView == null;

			VirtualView = view;
			if (PlatformView is null)
			{
				_handlerState = ElementHandlerState.Connecting;
				PlatformView = CreatePlatformElement();
			}
			else
			{
				_handlerState = ElementHandlerState.Reconnecting;
			}

			if (VirtualView.Handler != this)
			{
				VirtualView.Handler = this;
			}

			// We set the previous virtual view to null after setting it on the incoming virtual view.
			// This makes it easier for the incoming virtual view to have influence
			// on how the exchange of handlers happens.
			// We will just set the handler to null ourselves as a last resort cleanup
			if (oldVirtualView?.Handler != null)
			{
				oldVirtualView.Handler = null;
			}

			if (setupPlatformView)
			{
				ConnectHandler(PlatformView);
			}

			_mapper = _defaultMapper;

			if (VirtualView is IPropertyMapperView imv)
			{
				var map = imv.GetPropertyMapperOverrides();
				if (map is not null)
				{
					map.Chained = new[] { _defaultMapper };
					_mapper = map;
				}
			}

			_mapper.UpdateProperties(this, VirtualView);

			_handlerState = ElementHandlerState.Connected;
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

			_commandMapper?.Invoke(this, VirtualView, command, args);
		}

		private protected abstract object OnCreatePlatformElement();

		object CreatePlatformElement() =>
			OnCreatePlatformElement();

		private protected abstract void OnConnectHandler(object platformView);

		void ConnectHandler(object platformView) =>
			OnConnectHandler(platformView);

		private protected abstract void OnDisconnectHandler(object platformView);

		void DisconnectHandler(object platformView)
		{
			OnDisconnectHandler(platformView);

			// VirtualView has already been changed over to a new handler
			if (VirtualView != null && VirtualView.Handler == this)
				VirtualView.Handler = null;

			VirtualView = null;
		}

		void IElementHandler.DisconnectHandler()
		{
			if (PlatformView != null && VirtualView != null)
			{
				// We set the PlatformView to null so no one outside of this handler tries to access
				// PlatformView. PlatformView access should be isolated to the instance passed into
				// DisconnectHandler
				var oldPlatformView = PlatformView;
				PlatformView = null;
				DisconnectHandler(oldPlatformView);
			}

			_handlerState = ElementHandlerState.Disconnected;
		}
	}
}
