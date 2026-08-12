using System;
using System.Collections.Generic;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ElementHandler : IElementHandler, IElementHandlerStateExhibitor, IPropertyUpdateBatchingHandler
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
		HashSet<string>? _pendingPropertyUpdates;
		HashSet<string>? _mappedAutomaticPropertyUpdates;
		IElement? _pendingPropertyUpdateView;
		bool _isFlushingPropertyUpdates;
		bool _isApplyingLeadingAutomaticUpdate;
		bool _isMeasureInvalidationPending;
		bool _mappedAutomaticMeasureInvalidation;
		bool _isPropertyUpdateFlushScheduled;
		int _propertyUpdateFlushGeneration;

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

			ClearPendingPropertyUpdates();

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

			if (_isFlushingPropertyUpdates)
			{
				QueuePropertyUpdate(property);
				return;
			}

			if (VirtualView is IPropertyUpdateBatchingElement { IsPropertyUpdateBatchingEnabled: true } batchingElement)
			{
				if (!batchingElement.IsPropertyUpdateBatchingExplicitlyScoped && _handlerState != ElementHandlerState.Connected)
				{
					_mapper?.UpdateProperty(this, VirtualView, property);
					return;
				}

				if (!ReferenceEquals(_pendingPropertyUpdateView, VirtualView))
				{
					ClearPendingPropertyUpdates();
					_pendingPropertyUpdateView = VirtualView;
				}

				if (batchingElement.IsPropertyUpdateBatchingExplicitlyScoped)
				{
					QueuePropertyUpdate(property);
				}
				else if (_mappedAutomaticPropertyUpdates is not { Count: > 0 })
				{
					(_mappedAutomaticPropertyUpdates ??= new(StringComparer.Ordinal)).Add(property);
					SchedulePropertyUpdateFlush();
					var wasApplyingLeadingUpdate = _isApplyingLeadingAutomaticUpdate;
					_isApplyingLeadingAutomaticUpdate = true;
					try
					{
						_mapper?.UpdateProperty(this, VirtualView, property);
					}
					finally
					{
						_isApplyingLeadingAutomaticUpdate = wasApplyingLeadingUpdate;
					}
				}
				else
				{
					QueuePropertyUpdate(property);
				}

				return;
			}

			_mapper?.UpdateProperty(this, VirtualView, property);
		}

		public virtual void Invoke(string command, object? args)
		{
			if (VirtualView == null)
				return;

			if (command == nameof(IView.InvalidateMeasure) &&
				VirtualView is IPropertyUpdateBatchingElement { IsPropertyUpdateBatchingEnabled: true } batchingElement)
			{
				if (!batchingElement.IsPropertyUpdateBatchingExplicitlyScoped && _handlerState != ElementHandlerState.Connected)
				{
					_commandMapper?.Invoke(this, VirtualView, command, args);
					return;
				}

				if (!ReferenceEquals(_pendingPropertyUpdateView, VirtualView))
				{
					ClearPendingPropertyUpdates();
					_pendingPropertyUpdateView = VirtualView;
				}

				if (batchingElement.IsPropertyUpdateBatchingExplicitlyScoped)
				{
					_isMeasureInvalidationPending = true;
				}
				else if (!_mappedAutomaticMeasureInvalidation)
				{
					_mappedAutomaticMeasureInvalidation = true;
					SchedulePropertyUpdateFlush();
					var wasApplyingLeadingUpdate = _isApplyingLeadingAutomaticUpdate;
					_isApplyingLeadingAutomaticUpdate = true;
					try
					{
						_commandMapper?.Invoke(this, VirtualView, command, args);
					}
					finally
					{
						_isApplyingLeadingAutomaticUpdate = wasApplyingLeadingUpdate;
					}
				}
				else
				{
					_isMeasureInvalidationPending = _pendingPropertyUpdates is { Count: > 0 };
				}

				return;
			}

			FlushPendingPropertyUpdates();
			_commandMapper?.Invoke(this, VirtualView, command, args);
		}

		void QueuePropertyUpdate(string property)
		{
			if (!ReferenceEquals(_pendingPropertyUpdateView, VirtualView))
			{
				ClearPendingPropertyUpdates();
				_pendingPropertyUpdateView = VirtualView;
			}

			(_pendingPropertyUpdates ??= new(StringComparer.Ordinal)).Add(property);
		}

		void IPropertyUpdateBatchingHandler.FlushPendingPropertyUpdates() =>
			FlushPendingPropertyUpdates();

		void SchedulePropertyUpdateFlush()
		{
			if (_isPropertyUpdateFlushScheduled)
				return;

			_isPropertyUpdateFlushScheduled = true;
			var generation = ++_propertyUpdateFlushGeneration;

			var dispatcher = MauiContext?.Services?.GetService(typeof(IDispatcher)) as IDispatcher;
			if (dispatcher?.Dispatch(() => FlushScheduledPropertyUpdates(generation)) != true)
				FlushScheduledPropertyUpdates(generation);
		}

		void FlushScheduledPropertyUpdates(int generation)
		{
			if (generation != _propertyUpdateFlushGeneration)
				return;

			_isPropertyUpdateFlushScheduled = false;

			if (VirtualView is IPropertyUpdateBatchingElement { IsPropertyUpdateBatchingExplicitlyScoped: true })
				return;

			FlushPendingPropertyUpdates();
			if (_mappedAutomaticPropertyUpdates is { Count: > 0 })
				ClearPendingPropertyUpdates();
		}

		private protected void FlushPendingPropertyUpdatesBeforePlatformViewAccess()
		{
			if (!_isFlushingPropertyUpdates)
				FlushPendingPropertyUpdates();
		}

		internal void FlushPendingPropertyUpdates()
		{
			var pending = _pendingPropertyUpdates;
			if (pending is not { Count: > 0 } && !_isMeasureInvalidationPending)
			{
				if (!_isApplyingLeadingAutomaticUpdate &&
					(_mappedAutomaticPropertyUpdates is { Count: > 0 } || _mappedAutomaticMeasureInvalidation))
				{
					ClearPendingPropertyUpdates();
				}

				return;
			}

			if (VirtualView is null || !ReferenceEquals(_pendingPropertyUpdateView, VirtualView))
			{
				ClearPendingPropertyUpdates();
				return;
			}

			_isFlushingPropertyUpdates = true;

			try
			{
				const int maxPasses = 4;

				for (int pass = 0; pending is not null && pass < maxPasses && pending.Count > 0; pass++)
				{
					foreach (var key in _mapper.GetKeys())
					{
						if (pending.Remove(key))
							_mapper.GetProperty(key)?.Invoke(this, VirtualView);
					}
				}

				if (pending is { Count: > 0 })
				{
					var remaining = new string[pending.Count];
					pending.CopyTo(remaining);
					pending.Clear();

					foreach (var key in remaining)
						_mapper.GetProperty(key)?.Invoke(this, VirtualView);
				}

				if (_isMeasureInvalidationPending)
				{
					_isMeasureInvalidationPending = false;
					_commandMapper?.Invoke(this, VirtualView, nameof(IView.InvalidateMeasure), null);
				}
			}
			finally
			{
				_isFlushingPropertyUpdates = false;

				if (pending is { Count: > 0 } || _isMeasureInvalidationPending)
					SchedulePropertyUpdateFlush();
				else
					ClearPendingPropertyUpdates();
			}
		}

		void ClearPendingPropertyUpdates()
		{
			_pendingPropertyUpdates?.Clear();
			_mappedAutomaticPropertyUpdates?.Clear();
			_pendingPropertyUpdateView = null;
			_isMeasureInvalidationPending = false;
			_mappedAutomaticMeasureInvalidation = false;
			_isPropertyUpdateFlushScheduled = false;
			_propertyUpdateFlushGeneration++;
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
			ClearPendingPropertyUpdates();
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
