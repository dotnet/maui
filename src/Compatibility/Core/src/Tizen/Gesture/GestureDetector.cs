using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class GestureDetector
	{
		Dictionary<IGestureRecognizer, GestureHandler> _handlers = new Dictionary<IGestureRecognizer, GestureHandler>();

		IVisualElementRenderer _renderer;

		bool _isEnabled = true;

		public GestureDetector(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
		}

		public bool IsEnabled
		{
			get => _isEnabled;
			set
			{
				if (_isEnabled != value)
				{
					_isEnabled = value;
					UpdateIsEnabled();
				}
			}
		}

		public void AddGestures(IEnumerable<IGestureRecognizer> gestures)
		{
			foreach (var gesture in gestures)
			{
				AddGesture(gesture);
			}
		}

		public void AddGesture(IGestureRecognizer gesture)
		{
			var handler = CreateHandler(gesture);
			if (handler == null)
				return;

			_handlers.Add(gesture, handler);

			if (IsEnabled)
				handler.Attach(_renderer);
		}

		public void RemoveGestures(IEnumerable<IGestureRecognizer> gestures)
		{
			foreach (var gesture in gestures)
			{
				RemoveGesture(gesture);
			}
		}

		public void RemoveGesture(IGestureRecognizer gesture)
		{
			if (_handlers.TryGetValue(gesture, out GestureHandler handler))
			{
				_handlers.Remove(gesture);
				handler.Dispose();
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

		void UpdateIsEnabled()
		{
			if (IsEnabled)
			{
				foreach (var handler in _handlers.Values)
				{
					handler.Attach(_renderer);
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
			return Forms.GetHandlerForObject<GestureHandler>(recognizer, recognizer);
		}

	}
}
