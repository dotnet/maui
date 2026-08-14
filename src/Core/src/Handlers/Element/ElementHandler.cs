using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
		LinkedList<string>? _pendingPropertyUpdates;
		Dictionary<string, LinkedListNode<string>>? _pendingPropertyUpdateNodes;
		IElement? _pendingPropertyUpdateView;
		bool _isPropertyUpdateBatchingEnabled;
		bool _isAutomaticPropertyUpdateBatchingEnabled;
		bool _isExplicitPropertyUpdateBatching;
		bool _hasMappedAutomaticLeadingUpdate;
		bool _isPropertyUpdateFlushScheduled;
		int _propertyUpdateFlushGeneration;
		int _mapperExecutionDepth;

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
			if (view is IPropertyUpdateBatchingElement batchingElement)
			{
				_isPropertyUpdateBatchingEnabled = batchingElement.IsPropertyUpdateBatchingEnabled;
				_isAutomaticPropertyUpdateBatchingEnabled = batchingElement.IsAutomaticPropertyUpdateBatchingEnabled;
				_isExplicitPropertyUpdateBatching =
					_isPropertyUpdateBatchingEnabled &&
					batchingElement.IsPropertyUpdateBatchingExplicitlyScoped;
			}
			else
			{
				_isPropertyUpdateBatchingEnabled = false;
				_isAutomaticPropertyUpdateBatchingEnabled = false;
				_isExplicitPropertyUpdateBatching = false;
			}

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

			_mapperExecutionDepth++;
			try
			{
				_mapper.UpdateProperties(this, VirtualView);
			}
			finally
			{
				_mapperExecutionDepth--;
			}

			_handlerState = ElementHandlerState.Connected;
		}

		public virtual void UpdateValue(string property)
		{
			if (VirtualView == null)
				return;

			if (_mapperExecutionDepth > 0)
			{
				InvokePropertyMapper(property);
				return;
			}

			if (_isExplicitPropertyUpdateBatching || _isAutomaticPropertyUpdateBatchingEnabled)
			{
				if (!_isExplicitPropertyUpdateBatching && _handlerState != ElementHandlerState.Connected)
				{
					_mapper?.UpdateProperty(this, VirtualView, property);
					return;
				}

				if (!ReferenceEquals(_pendingPropertyUpdateView, VirtualView))
				{
					ClearPendingPropertyUpdates();
					_pendingPropertyUpdateView = VirtualView;
				}

				if (_isExplicitPropertyUpdateBatching)
				{
					QueuePropertyUpdate(property);
				}
				else if (!_hasMappedAutomaticLeadingUpdate)
				{
					_hasMappedAutomaticLeadingUpdate = true;
					SchedulePropertyUpdateFlush();
					InvokePropertyMapper(property);
				}
				else
				{
					QueuePropertyUpdate(property);
				}

				return;
			}

			InvokePropertyMapper(property);
		}

		public virtual void Invoke(string command, object? args)
		{
			if (VirtualView == null)
				return;

			if (command == nameof(IView.InvalidateMeasure))
			{
				InvokeCommandMapper(command, args);
				return;
			}

			if (_mapperExecutionDepth == 0)
				FlushPendingPropertyUpdates();

			InvokeCommandMapper(command, args);
		}

		void QueuePropertyUpdate(string property)
		{
			if (!ReferenceEquals(_pendingPropertyUpdateView, VirtualView))
			{
				ClearPendingPropertyUpdates();
				_pendingPropertyUpdateView = VirtualView;
			}

			_pendingPropertyUpdates ??= new();
			_pendingPropertyUpdateNodes ??= new(StringComparer.Ordinal);

			if (_pendingPropertyUpdateNodes.TryGetValue(property, out var existingNode))
			{
				_pendingPropertyUpdates.Remove(existingNode);
				_pendingPropertyUpdates.AddLast(existingNode);
			}
			else
			{
				var node = _pendingPropertyUpdates.AddLast(property);
				_pendingPropertyUpdateNodes.Add(property, node);
			}
		}

		void IPropertyUpdateBatchingHandler.BeginPropertyUpdateBatch()
		{
			if (_isPropertyUpdateBatchingEnabled)
				_isExplicitPropertyUpdateBatching = true;
		}

		void IPropertyUpdateBatchingHandler.FlushPendingPropertyUpdates()
		{
			_isExplicitPropertyUpdateBatching = false;
			FlushPendingPropertyUpdates();
		}

		void SchedulePropertyUpdateFlush()
		{
			if (_isPropertyUpdateFlushScheduled)
				return;

			_isPropertyUpdateFlushScheduled = true;
			var generation = ++_propertyUpdateFlushGeneration;

			var dispatcher = MauiContext?.GetOptionalDispatcher();
			if (dispatcher?.Dispatch(() => FlushScheduledPropertyUpdates(generation)) != true)
				FlushScheduledPropertyUpdates(generation);
		}

		void FlushScheduledPropertyUpdates(int generation)
		{
			if (generation != _propertyUpdateFlushGeneration)
				return;

			_isPropertyUpdateFlushScheduled = false;

			if (_isExplicitPropertyUpdateBatching)
				return;

			FlushPendingPropertyUpdates();
			if (_hasMappedAutomaticLeadingUpdate)
				ClearPendingPropertyUpdates();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected void FlushPendingPropertyUpdatesBeforePlatformViewAccess()
		{
			if (_mapperExecutionDepth == 0 && _pendingPropertyUpdates is { Count: > 0 })
				FlushPendingPropertyUpdates();
		}

		internal void FlushPendingPropertyUpdates()
		{
			var pending = _pendingPropertyUpdates;
			if (pending is not { Count: > 0 })
			{
				if (_mapperExecutionDepth == 0 && _hasMappedAutomaticLeadingUpdate)
					ClearPendingPropertyUpdates();

				return;
			}

			if (_mapperExecutionDepth > 0)
				return;

			if (VirtualView is null || !ReferenceEquals(_pendingPropertyUpdateView, VirtualView))
			{
				ClearPendingPropertyUpdates();
				return;
			}

			try
			{
				while (pending.First is { } node)
				{
					var property = node.Value;
					pending.RemoveFirst();
					_pendingPropertyUpdateNodes!.Remove(property);
					InvokeCurrentPropertyMapper(property);
				}
			}
			finally
			{
				ClearPendingPropertyUpdates();
			}
		}

		void InvokePropertyMapper(string property)
		{
			_mapperExecutionDepth++;
			try
			{
				_mapper?.UpdateProperty(this, VirtualView!, property);
			}
			finally
			{
				_mapperExecutionDepth--;
			}
		}

		void InvokeCurrentPropertyMapper(string property)
		{
			_mapperExecutionDepth++;
			try
			{
				if (!this.CanInvokeMappers())
					return;

				_mapper.GetProperty(property)?.Invoke(this, VirtualView!);
			}
			finally
			{
				_mapperExecutionDepth--;
			}
		}

		void InvokeCommandMapper(string command, object? args)
		{
			_mapperExecutionDepth++;
			try
			{
				_commandMapper?.Invoke(this, VirtualView!, command, args);
			}
			finally
			{
				_mapperExecutionDepth--;
			}
		}

		void ClearPendingPropertyUpdates()
		{
			_pendingPropertyUpdates?.Clear();
			_pendingPropertyUpdateNodes?.Clear();
			_pendingPropertyUpdateView = null;
			_hasMappedAutomaticLeadingUpdate = false;
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
			_isPropertyUpdateBatchingEnabled = false;
			_isAutomaticPropertyUpdateBatchingEnabled = false;
			_isExplicitPropertyUpdateBatching = false;
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
