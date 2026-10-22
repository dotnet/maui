using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	class GestureDetector : IDisposable
	{
		Dictionary<IGestureRecognizer, GestureHandler> _handlers = new Dictionary<IGestureRecognizer, GestureHandler>();

		IViewHandler? _handler;
		bool _inputTransparent = false;
		bool _isEnabled;

		protected virtual VisualElement? Element => _handler?.VirtualView as VisualElement;
		View? View => Element as View;

		public bool IsEnabled
		{
			get
			{
				return _isEnabled;
			}
			set
			{
				_isEnabled = value;
				UpdateIsEnabled();
			}
		}

		public bool InputTransparent
		{
			get
			{
				return _inputTransparent;
			}
			set
			{
				_inputTransparent = value;
				UpdateIsEnabled();
			}
		}

		bool GestureEnabled => IsEnabled && !InputTransparent;

		public GestureDetector(IViewHandler? handler)
		{
			_handler = handler;
			_isEnabled = View?.IsEnabled ?? false;
			_inputTransparent = View?.InputTransparent ?? false;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void AddGestures(IEnumerable<IGestureRecognizer>? gestures)
		{
			if (gestures == null)
				return;
			foreach (var gesture in gestures)
			{
				AddGesture(gesture);
			}
		}

		public void RemoveGestures(IEnumerable<IGestureRecognizer>? gestures)
		{
			if (gestures == null)
				return;
			foreach (var gesture in gestures)
			{
				RemoveGesture(gesture);
			}
		}

		public void Clear()
		{
			foreach (var handler in _handlers)
			{
				handler.Value.Dispose();
			}
			_handlers.Clear();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Clear();
				_handler = null;
			}
		}

		void AddGesture(IGestureRecognizer gesture)
		{
			var handler = CreateHandler(gesture);
			if (handler == null)
				return;

			_handlers.Add(gesture, handler);

			if (GestureEnabled)
				handler.Attach(_handler!);
		}

		void RemoveGesture(IGestureRecognizer gesture)
		{
			if (_handlers.TryGetValue(gesture, out GestureHandler? handler))
			{
				_handlers.Remove(gesture);
				handler.Dispose();
			}
		}

		void UpdateIsEnabled()
		{
			if (GestureEnabled)
			{
				foreach (var handler in _handlers.Values)
				{
					handler.Attach(_handler!);
				}
			}
			else
			{
				foreach (var handler in _handlers.Values)
				{
					handler.Detach();
				}
			}
		}

		GestureHandler CreateHandler(IGestureRecognizer recognizer)
		{
			if (recognizer is TapGestureRecognizer)
				return new TapGestureHandler(recognizer);
			if (recognizer is PanGestureRecognizer)
				return new PanGestureHandler(recognizer);
			if (recognizer is PinchGestureRecognizer)
				return new PinchGestureHandler(recognizer);
			if (recognizer is SwipeGestureRecognizer)
				return new SwipeGestureHandler(recognizer);
			if (recognizer is LongPressGestureRecognizer)
				return new LongPressGestureHandler(recognizer);
			return Registrar.Registered.GetHandlerForObject<GestureHandler>(recognizer, recognizer);
		}

	}
}
