using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using ElmSharp;
using Xamarin.Forms.Internals;
using EGestureType = ElmSharp.GestureLayer.GestureType;

namespace Xamarin.Forms.Platform.Tizen
{
	internal class GestureDetector
	{
		IDictionary<EGestureType, List<GestureHandler>> _handlerCache = new Dictionary<EGestureType, List<GestureHandler>>();

		readonly IVisualElementRenderer _renderer;
		GestureLayer _gestureLayer;
		double _doubleTapTime = 0;
		double _longTapTime = 0;
		int _horizontalSwipeTime = 0;
		int _verticalSwipeTime = 0;
		bool _inputTransparent = false;
		bool _isEnabled = true;

		View View => _renderer.Element as View;

		public bool IsEnabled
		{
			get
			{
				return _isEnabled;
			}
			set
			{
				_isEnabled = value;
				UpdateGestureLayerEnabled();
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
				UpdateGestureLayerEnabled();
			}
		}

		public GestureDetector(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
			_isEnabled = View.IsEnabled;
			_inputTransparent = View.InputTransparent;

			(View.GestureRecognizers as ObservableCollection<IGestureRecognizer>).CollectionChanged += OnGestureRecognizerCollectionChanged;

			if (View.GestureRecognizers.Count > 0)
			{
				CreateGestureLayer();
				AddGestures(View.GestureRecognizers);
			}
		}

		public void Clear()
		{
			// this will clear all callbacks in ElmSharp GestureLayer
			_gestureLayer?.Unrealize();
			_gestureLayer = null;
			foreach (var handlers in _handlerCache.Values)
			{
				foreach (var handler in handlers)
				{
					(handler as GestureHandler).PropertyChanged -= OnGestureRecognizerPropertyChanged;
				}
			}
			_handlerCache.Clear();
		}

		void CreateGestureLayer()
		{
			_gestureLayer = new GestureLayer(_renderer.NativeView);
			_gestureLayer.Attach(_renderer.NativeView);
			_gestureLayer.Deleted += (s, e) =>
			{
				_gestureLayer = null;
				Clear();
			};
			UpdateGestureLayerEnabled();
		}

		void UpdateGestureLayerEnabled()
		{
			if (_gestureLayer != null)
			{
				_gestureLayer.IsEnabled = !_inputTransparent && _isEnabled;
			}
		}


		void AddGestures(IEnumerable<IGestureRecognizer> recognizers)
		{
			foreach (var item in recognizers)
				AddGesture(item);
		}

		void RemoveGestures(IEnumerable<IGestureRecognizer> recognizers)
		{
			foreach (var item in recognizers)
				RemoveGesture(item);
		}

		void AddGesture(IGestureRecognizer recognizer)
		{
			var handler = CreateHandler(recognizer);
			if (handler == null)
				return;

			var gestureType = handler.Type;
			var timeout = handler.Timeout;
			var cache = _handlerCache;

			if (!cache.ContainsKey(gestureType))
			{
				cache[gestureType] = new List<GestureHandler>();
			}

			handler.PropertyChanged += OnGestureRecognizerPropertyChanged;
			cache[gestureType].Add(handler);

			if (cache[gestureType].Count == 1)
			{
				switch (gestureType)
				{
					case EGestureType.Tap:
					case EGestureType.TripleTap:
						AddTapGesture(gestureType);
						break;

					case EGestureType.DoubleTap:
						AddDoubleTapGesture(gestureType, timeout);
						break;

					case EGestureType.LongTap:
						AddLongTapGesture(gestureType, timeout);
						break;

					case EGestureType.Line:
						AddLineGesture(gestureType);
						break;

					case EGestureType.Flick:
						AddFlickGesture(gestureType, timeout);
						break;

					case EGestureType.Rotate:
						AddRotateGesture(gestureType);
						break;

					case EGestureType.Momentum:
						AddMomentumGesture(gestureType);
						break;

					case EGestureType.Zoom:
						AddPinchGesture(gestureType);
						break;

					default:
						break;
				}
			}
		}

		void RemoveGesture(IGestureRecognizer recognizer)
		{
			var cache = _handlerCache;
			var handler = LookupHandler(recognizer);
			var gestureType = cache.FirstOrDefault(x => x.Value.Contains(handler)).Key;

			handler.PropertyChanged -= OnGestureRecognizerPropertyChanged;
			cache[gestureType].Remove(handler);

			if (cache[gestureType].Count == 0)
			{
				switch (gestureType)
				{
					case EGestureType.Tap:
					case EGestureType.DoubleTap:
					case EGestureType.TripleTap:
					case EGestureType.LongTap:
						RemoveTapGesture(gestureType);
						break;

					case EGestureType.Line:
						RemoveLineGesture();
						break;

					case EGestureType.Flick:
						RemoveFlickGesture();
						break;

					case EGestureType.Rotate:
						RemoveRotateGesture();
						break;

					case EGestureType.Momentum:
						RemoveMomentumGesture();
						break;

					case EGestureType.Zoom:
						RemovePinchGesture();
						break;

					default:
						break;
				}
			}
		}

		void AddLineGesture(EGestureType type)
		{
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Start, (data) => { OnGestureStarted(type, data); });
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Move, (data) => { OnGestureMoved(type, data); });
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.End, (data) => { OnGestureCompleted(type, data); });
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void AddPinchGesture(EGestureType type)
		{
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.Start, (data) => { OnGestureStarted(type, data); });
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.Move, (data) => { OnGestureMoved(type, data); });
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.End, (data) => { OnGestureCompleted(type, data); });
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void AddTapGesture(EGestureType type)
		{
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Start, (data) => { OnGestureStarted(type, data); });
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.End, (data) => { OnGestureCompleted(type, data); });
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void AddDoubleTapGesture(EGestureType type, double timeout)
		{
			if (timeout > 0)
				_gestureLayer.DoubleTapTimeout = timeout;

			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Start, (data) => { OnDoubleTapStarted(type, data); });
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.End, (data) => { OnDoubleTapCompleted(type, data); });
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void AddLongTapGesture(EGestureType type, double timeout)
		{
			if (timeout > 0)
				_gestureLayer.LongTapTimeout = timeout;

			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Start, (data) => { OnLongTapStarted(type, data); });
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.End, (data) => { OnLongTapCompleted(type, data); });
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void AddFlickGesture(EGestureType type, double timeout)
		{
			if (timeout > 0)
				_gestureLayer.FlickTimeLimit = (int)(timeout * 1000);

			// Task to correct wrong coordinates information when applying EvasMap(Xamarin ex: Translation, Scale, Rotate property)
			// Always change to the absolute coordinates of the pointer.
			int startX = 0;
			int startY = 0;
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.Start, (data) =>
			{
				startX = _gestureLayer.EvasCanvas.Pointer.X;
				startY = _gestureLayer.EvasCanvas.Pointer.Y;
				data.X1 = startX;
				data.Y1 = startY;
				OnGestureStarted(type, data);
			});
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.Move, (data) =>
			{
				data.X1 = startX;
				data.Y1 = startY;
				data.X2 = _gestureLayer.EvasCanvas.Pointer.X;
				data.Y2 = _gestureLayer.EvasCanvas.Pointer.Y;
				OnGestureMoved(type, data);
			});
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.End, (data) =>
			{
				data.X1 = startX;
				data.Y1 = startY;
				data.X2 = _gestureLayer.EvasCanvas.Pointer.X;
				data.Y2 = _gestureLayer.EvasCanvas.Pointer.Y;
				OnGestureCompleted(type, data);
			});
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void AddRotateGesture(EGestureType type)
		{
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.Start, (data) => { OnGestureStarted(type, data); });
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.Move, (data) => { OnGestureMoved(type, data); });
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.End, (data) => { OnGestureCompleted(type, data); });
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void AddMomentumGesture(EGestureType type)
		{
			// Task to correct wrong coordinates information when applying EvasMap(Xamarin ex: Translation, Scale, Rotate property)
			// Always change to the absolute coordinates of the pointer.
			int startX = 0;
			int startY = 0;
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Start, (data) =>
			{
				startX = _gestureLayer.EvasCanvas.Pointer.X;
				startY = _gestureLayer.EvasCanvas.Pointer.Y;
				OnGestureStarted(type, data);
			});
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Move, (data) =>
			{
				data.X1 = startX;
				data.Y1 = startY;
				data.X2 = _gestureLayer.EvasCanvas.Pointer.X;
				data.Y2 = _gestureLayer.EvasCanvas.Pointer.Y;
				OnGestureMoved(type, data);
			});
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.End, (data) => { OnGestureCompleted(type, data); });
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Abort, (data) => { OnGestureCanceled(type, data); });
		}

		void RemoveLineGesture()
		{
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Start, null);
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Move, null);
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.End, null);
			_gestureLayer.SetLineCallback(GestureLayer.GestureState.Abort, null);
		}

		void RemovePinchGesture()
		{
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.Start, null);
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.Move, null);
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.End, null);
			_gestureLayer.SetZoomCallback(GestureLayer.GestureState.Abort, null);
		}

		void RemoveTapGesture(EGestureType type)
		{
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Start, null);
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.End, null);
			_gestureLayer.SetTapCallback(type, GestureLayer.GestureState.Abort, null);
		}

		void RemoveFlickGesture()
		{
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.Start, null);
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.Move, null);
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.End, null);
			_gestureLayer.SetFlickCallback(GestureLayer.GestureState.Abort, null);
		}

		void RemoveRotateGesture()
		{
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.Start, null);
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.Move, null);
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.End, null);
			_gestureLayer.SetRotateCallback(GestureLayer.GestureState.Abort, null);
		}

		void RemoveMomentumGesture()
		{
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Start, null);
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Move, null);
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.End, null);
			_gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Abort, null);
		}

		#region GestureCallback

		void OnGestureStarted(EGestureType type, object data)
		{
			var cache = _handlerCache;
			if (cache.ContainsKey(type))
			{
				foreach (var handler in cache[type])
				{
					(handler as IGestureController)?.SendStarted(View, data);
				}
			}
		}

		void OnGestureMoved(EGestureType type, object data)
		{
			var cache = _handlerCache;
			if (cache.ContainsKey(type))
			{
				foreach (var handler in cache[type])
				{
					(handler as IGestureController)?.SendMoved(View, data);
				}
			}
		}

		void OnGestureCompleted(EGestureType type, object data)
		{
			var cache = _handlerCache;
			if (cache.ContainsKey(type))
			{
				foreach (var handler in cache[type])
				{
					(handler as IGestureController)?.SendCompleted(View, data);
				}
			}
		}

		void OnGestureCanceled(EGestureType type, object data)
		{
			var cache = _handlerCache;
			if (cache.ContainsKey(type))
			{
				foreach (var handler in cache[type])
				{
					(handler as IGestureController)?.SendCanceled(View, data);
				}
			}
		}

		void OnDoubleTapStarted(EGestureType type, object data)
		{
			_doubleTapTime = ((GestureLayer.TapData)data).Timestamp;
			OnGestureStarted(type, data);
		}

		void OnDoubleTapCompleted(EGestureType type, object data)
		{
			_doubleTapTime = ((GestureLayer.TapData)data).Timestamp - _doubleTapTime;
			var cache = _handlerCache;

			if (cache.ContainsKey(type))
			{
				foreach (var handler in cache[type])
				{
					if ((handler.Timeout * 1000) >= _longTapTime)
						(handler as IGestureController)?.SendCompleted(View, data);
					else
						(handler as IGestureController)?.SendCanceled(View, data);
				}
			}
		}

		void OnLongTapStarted(EGestureType type, object data)
		{
			_longTapTime = ((GestureLayer.TapData)data).Timestamp;
			OnGestureStarted(type, data);
		}

		void OnLongTapCompleted(EGestureType type, object data)
		{
			_longTapTime = ((GestureLayer.TapData)data).Timestamp - _longTapTime;
			var cache = _handlerCache;

			if (cache.ContainsKey(type))
			{
				foreach (var handler in cache[type])
				{
					if ((handler.Timeout * 1000) <= _longTapTime)
						(handler as IGestureController)?.SendCompleted(View, data);
					else
						(handler as IGestureController)?.SendCanceled(View, data);
				}
			}
		}

		void OnFlickStarted(EGestureType type, object data)
		{
			var lineData = (GestureLayer.LineData)data;
			_horizontalSwipeTime = Convert.ToInt32(lineData.HorizontalSwipeTimestamp);
			_verticalSwipeTime = Convert.ToInt32(lineData.VerticalSwipeTimestamp);
			OnGestureStarted(type, data);
		}

		void OnFlickCompleted(EGestureType type, object data)
		{
			var lineData = (GestureLayer.LineData)data;
			_horizontalSwipeTime = Convert.ToInt32(lineData.HorizontalSwipeTimestamp - _horizontalSwipeTime);
			_verticalSwipeTime = Convert.ToInt32(lineData.VerticalSwipeTimestamp - _verticalSwipeTime);
			var cache = _handlerCache;

			if (cache.ContainsKey(type))
			{
				foreach (var handler in cache[type])
				{
					if ((handler.Timeout * 1000) >= _horizontalSwipeTime ||
						(handler.Timeout * 1000) >= _verticalSwipeTime)
						(handler as IGestureController)?.SendCompleted(View, data);
					else
						(handler as IGestureController)?.SendCanceled(View, data);
				}
			}
		}

		#endregion GestureCallback

		GestureHandler CreateHandler(IGestureRecognizer recognizer)
		{
			return Registrar.Registered.GetHandlerForObject<GestureHandler>(recognizer, recognizer);
		}

		GestureHandler LookupHandler(IGestureRecognizer recognizer)
		{
			var cache = _handlerCache;

			foreach (var handlers in cache.Values)
			{
				foreach (var handler in handlers)
				{
					if (handler.Recognizer == recognizer)
						return handler;
				}
			}
			return null;
		}

		void UpdateTapGesture(GestureHandler handler)
		{
			RemoveGesture(handler.Recognizer);
			AddGesture(handler.Recognizer);

			if (handler.Timeout > _gestureLayer.DoubleTapTimeout)
				_gestureLayer.DoubleTapTimeout = handler.Timeout;
		}

		void UpdateLongTapGesture(GestureHandler handler)
		{
			if (handler.Timeout > 0 && handler.Timeout < _gestureLayer.LongTapTimeout)
				_gestureLayer.LongTapTimeout = handler.Timeout;
		}

		void UpdateFlickGesture(GestureHandler handler)
		{
			if (handler.Timeout > _gestureLayer.FlickTimeLimit)
				_gestureLayer.FlickTimeLimit = (int)(handler.Timeout * 1000);
		}

		void OnGestureRecognizerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var handler = sender as GestureHandler;
			if (handler != null)
			{
				switch (handler.Type)
				{
					case EGestureType.Tap:
					case EGestureType.DoubleTap:
					case EGestureType.TripleTap:
						UpdateTapGesture(handler);
						break;

					case EGestureType.LongTap:
						UpdateLongTapGesture(handler);
						break;

					case EGestureType.Flick:
						UpdateFlickGesture(handler);
						break;

					default:
						break;
				}
			}
		}

		void OnGestureRecognizerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// Gestures will be registered/unregistered according to changes in the GestureRecognizers list
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (_gestureLayer == null)
						CreateGestureLayer();
					AddGestures(e.NewItems.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveGestures(e.OldItems.OfType<IGestureRecognizer>());
					AddGestures(e.NewItems.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveGestures(e.OldItems.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Reset:
					Clear();
					break;
			}
		}
	}
}