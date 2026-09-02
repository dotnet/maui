using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
		List<PendingPropertyUpdate>? _pendingPropertyUpdates;
		Dictionary<string, int>? _pendingPropertyUpdateIndices;
		IElement? _pendingPropertyUpdateView;
		Action? _scheduledPropertyUpdateFlushCallback;
		bool _isPropertyUpdateBatchingEnabled;
		bool _isAutomaticPropertyUpdateBatchingEnabled;
		bool _isExplicitPropertyUpdateBatching;
		bool _hasMappedAutomaticLeadingUpdate;
		bool _isPropertyUpdateFlushScheduled;
		bool _isExplicitFlushPendingUnwind;
		int _pendingPropertyUpdateHead = -1;
		int _pendingPropertyUpdateTail = -1;
		int _mapperExecutionDepth;
		object? _platformView;

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

		// This is the single canonical flush barrier for platform-view access: every other
		// PlatformView/ContainerView accessor (ElementHandlerOfT, ViewHandler, ViewHandlerOfT)
		// funnels through this getter via `base.PlatformView`, so there is exactly one place
		// that needs to flush pending property updates before native code observes the view.
		public object? PlatformView
		{
			get
			{
				FlushPendingPropertyUpdatesBeforePlatformViewAccess();
				return _platformView;
			}
			private protected set => _platformView = value;
		}

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

			if (!_isExplicitPropertyUpdateBatching && !_isAutomaticPropertyUpdateBatchingEnabled)
			{
				_mapper.UpdateProperties(this, VirtualView);
			}
			else
			{
				_mapperExecutionDepth++;
				try
				{
					_mapper.UpdateProperties(this, VirtualView);
				}
				finally
				{
					_mapperExecutionDepth--;
					FlushPendingExplicitUpdatesIfUnwound();
				}
			}

			_handlerState = ElementHandlerState.Connected;
		}

		public virtual void UpdateValue(string property)
		{
			if (VirtualView == null)
				return;

			if (!_isExplicitPropertyUpdateBatching && !_isAutomaticPropertyUpdateBatchingEnabled)
			{
				_mapper?.UpdateProperty(this, VirtualView, property);
				return;
			}

			if (_mapperExecutionDepth > 0)
			{
				InvokePropertyMapper(property);
				return;
			}

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
				// Map the leading update before scheduling the flush. SchedulePropertyUpdateFlush
				// falls back to invoking its callback synchronously when no dispatcher is
				// available (or Dispatch fails), and that callback drains the (still-empty)
				// queue and clears _hasMappedAutomaticLeadingUpdate. If we scheduled first, that
				// synchronous fallback would wipe the flag before the leading property mapper
				// even ran, and every subsequent property would re-enter this branch -- never
				// coalescing anything, and re-attempting a schedule/flush round-trip per update.
				// Invoking first means the no-dispatcher fallback degrades to the simple
				// immediate-update path (matching pre-batching behavior) instead of a stuck state.
				_hasMappedAutomaticLeadingUpdate = true;
				InvokePropertyMapper(property);
				SchedulePropertyUpdateFlush();
			}
			else
			{
				QueuePropertyUpdate(property);
			}
		}

		public virtual void Invoke(string command, object? args)
		{
			if (VirtualView == null)
				return;

			if (!_isExplicitPropertyUpdateBatching && !_isAutomaticPropertyUpdateBatchingEnabled)
			{
				_commandMapper?.Invoke(this, VirtualView, command, args);
				return;
			}

			// Every command -- including InvalidateMeasure -- flushes pending property updates
			// first. Otherwise a platform command (e.g. a layout pass triggered by
			// InvalidateMeasure) could observe a platform view that still holds stale values
			// for properties queued earlier in the same batch. InvalidateMeasure itself is
			// still dispatched synchronously and is never queued/coalesced.
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

			var pending = _pendingPropertyUpdates ??= new();
			var indices = _pendingPropertyUpdateIndices ??= new(StringComparer.Ordinal);

			if (indices.TryGetValue(property, out var index))
			{
				if (index == _pendingPropertyUpdateTail)
					return;

				var node = pending[index];

				if (node.Previous >= 0)
				{
					var previous = pending[node.Previous];
					previous.Next = node.Next;
					pending[node.Previous] = previous;
				}
				else
				{
					_pendingPropertyUpdateHead = node.Next;
				}

				if (node.Next >= 0)
				{
					var next = pending[node.Next];
					next.Previous = node.Previous;
					pending[node.Next] = next;
				}

				var tail = pending[_pendingPropertyUpdateTail];
				tail.Next = index;
				pending[_pendingPropertyUpdateTail] = tail;

				node.Previous = _pendingPropertyUpdateTail;
				node.Next = -1;
				pending[index] = node;
				_pendingPropertyUpdateTail = index;
			}
			else
			{
				index = pending.Count;
				pending.Add(new(property, _pendingPropertyUpdateTail));
				indices.Add(property, index);

				if (_pendingPropertyUpdateTail >= 0)
				{
					var tail = pending[_pendingPropertyUpdateTail];
					tail.Next = index;
					pending[_pendingPropertyUpdateTail] = tail;
				}
				else
				{
					_pendingPropertyUpdateHead = index;
				}

				_pendingPropertyUpdateTail = index;
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

			if (_mapperExecutionDepth > 0)
			{
				// BatchCommit() was reached reentrantly from inside a mapper callback (for
				// example an animation tick or a property-changed handler reached synchronously
				// from mapper code). FlushPendingPropertyUpdates() would no-op here because it
				// refuses to recurse into mapper invocation while a mapper is still on the
				// stack, which would otherwise strand the queue with nothing left to trigger a
				// drain (explicit batching is now off, so UpdateValue no longer re-queues or
				// re-schedules). Instead, request a drain and perform it as soon as the
				// outermost mapper invocation unwinds back to depth 0.
				_isExplicitFlushPendingUnwind = true;
				return;
			}

			FlushPendingPropertyUpdates();
		}

		// Called after every mapper/command invocation unwinds one level. If a reentrant
		// explicit-batch commit requested a drain while depth was still non-zero, perform it
		// as soon as we're back at depth 0.
		void FlushPendingExplicitUpdatesIfUnwound()
		{
			if (_mapperExecutionDepth != 0 || !_isExplicitFlushPendingUnwind)
				return;

			_isExplicitFlushPendingUnwind = false;
			FlushPendingPropertyUpdates();
		}

		void SchedulePropertyUpdateFlush()
		{
			if (_isPropertyUpdateFlushScheduled)
				return;

			_isPropertyUpdateFlushScheduled = true;
			var callback = _scheduledPropertyUpdateFlushCallback ??= FlushScheduledPropertyUpdates;

			var dispatcher = MauiContext?.GetOptionalDispatcher();
			if (dispatcher?.Dispatch(callback) != true)
				callback();
		}

		void FlushScheduledPropertyUpdates()
		{
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
			if (_pendingPropertyUpdateIndices is { Count: > 0 } && _mapperExecutionDepth == 0)
				FlushPendingPropertyUpdates();
		}

		internal void FlushPendingPropertyUpdates()
		{
			if (_pendingPropertyUpdateIndices is not { Count: > 0 } indices)
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
				var pending = _pendingPropertyUpdates!;

				while (_pendingPropertyUpdateHead >= 0)
				{
					var index = _pendingPropertyUpdateHead;
					var node = pending[index];
					var property = node.Property!;
					_pendingPropertyUpdateHead = node.Next;
					indices.Remove(property);
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
				FlushPendingExplicitUpdatesIfUnwound();
			}
		}

		void InvokeCurrentPropertyMapper(string property)
		{
			_mapperExecutionDepth++;
			try
			{
				if (!this.CanInvokeMappers())
					return;

				// Deliberately use GetProperty (a live chain walk) instead of UpdateProperty's
				// flattened CachedMappers lookup. CachedMappers is memoized on this (the leaf)
				// mapper instance and is only invalidated when *this* mapper's own Add/Chained
				// setter runs -- mutating a chained mapper's registrations later (for example
				// AppendToMapping/PrependToMapping/ModifyMapping called on the chained mapper
				// after this handler has already snapshotted its cache) does not invalidate it.
				// Flushed properties must resolve the mapper action current as of flush time,
				// including such chained customization (see "Ordering and barriers" in
				// docs/design/HandlerUpdateBatching.md and
				// BatchedPropertyUpdatesResolveChainedMapperAtFlushTime); switching this to the
				// cached path would silently drop any chained-mapper customization applied
				// between when a property was queued and when it is flushed.
				_mapper.GetProperty(property)?.Invoke(this, VirtualView!);
			}
			finally
			{
				_mapperExecutionDepth--;
				FlushPendingExplicitUpdatesIfUnwound();
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
				FlushPendingExplicitUpdatesIfUnwound();
			}
		}

		void ClearPendingPropertyUpdates()
		{
			_pendingPropertyUpdates?.Clear();
			_pendingPropertyUpdateIndices?.Clear();
			_pendingPropertyUpdateView = null;
			_hasMappedAutomaticLeadingUpdate = false;
			_pendingPropertyUpdateHead = -1;
			_pendingPropertyUpdateTail = -1;

			// Reset the scheduling state along with the queue itself. Without this, a flush
			// triggered by a barrier (PlatformView/ContainerView access), an explicit commit, or
			// a virtual-view/disconnect reset leaves _isPropertyUpdateFlushScheduled set to true
			// even though nothing is actually scheduled to run against the *new* queue contents.
			// SchedulePropertyUpdateFlush() would then early-return for the next leading update,
			// silently relying on a stale dispatcher callback (if any) to eventually flush
			// properties that callback was never queued for. Any already-dispatched callback
			// still safely no-ops against an empty/foreign queue when it eventually runs.
			_isPropertyUpdateFlushScheduled = false;
			_isExplicitFlushPendingUnwind = false;
		}

		struct PendingPropertyUpdate
		{
			public PendingPropertyUpdate(string property, int previous)
			{
				Property = property;
				Previous = previous;
				Next = -1;
			}

			public string? Property;
			public int Previous;
			public int Next;
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
			// Deliberately read/clear the raw field here, not the public PlatformView property.
			// Pending property updates are discarded (not flushed) on disconnect -- see
			// DisconnectHandler(object) below, which clears the queue via
			// ClearPendingPropertyUpdates(). Going through the public property would run the
			// platform-view-access flush barrier and apply queued updates to a handler that is
			// about to be torn down.
			if (_platformView != null && VirtualView != null)
			{
				// We set the PlatformView to null so no one outside of this handler tries to access
				// PlatformView. PlatformView access should be isolated to the instance passed into
				// DisconnectHandler
				var oldPlatformView = _platformView;
				_platformView = null;
				DisconnectHandler(oldPlatformView);
			}

			_handlerState = ElementHandlerState.Disconnected;
		}
	}
}
