using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Platform
{
	class GesturePlatformManager : IGesturePlatformManager
	{
		IViewHandler? _handler;
		Lazy<GestureDetector> _gestureDetector;
		bool _disposed = false;

		protected virtual VisualElement? Element => _handler?.VirtualView as VisualElement;

		public GesturePlatformManager(IViewHandler handler)
		{
			_handler = handler;
			_gestureDetector = new Lazy<GestureDetector>(() => new GestureDetector(handler));
			SetupElement(null, Element);
		}

		void SetupElement(VisualElement? oldElement, VisualElement? newElement)
		{
			if (oldElement != null)
			{
				if (oldElement is View ov &&
					ov.GestureRecognizers is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= OnGestureRecognizerCollectionChanged;
				}
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (newElement != null)
			{
				if (newElement is View ov &&
					ov.GestureRecognizers is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged += OnGestureRecognizerCollectionChanged;
					if (ov.GestureRecognizers.Count > 0)
					{
						_gestureDetector.Value.AddGestures(ov.GestureRecognizers);
					}
				}
				newElement.PropertyChanged += OnElementPropertyChanged;

				UpdateInputTransparent();
				UpdateIsEnabled();
			}
		}

		void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				SetupElement(Element, null);
				if (_gestureDetector.IsValueCreated)
				{
					_gestureDetector.Value.Dispose();
				}
				_handler = null;
			}
		}

		void UpdateInputTransparent()
		{
			if (Element != null && _gestureDetector.IsValueCreated)
			{
				_gestureDetector.Value.InputTransparent = Element.InputTransparent;
			}
		}

		void UpdateIsEnabled()
		{
			if (Element != null && _gestureDetector.IsValueCreated)
			{
				_gestureDetector.Value.IsEnabled = Element.IsEnabled;
			}
		}

		void OnGestureRecognizerCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			// Gestures will be registered/unregistered according to changes in the GestureRecognizers list
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					_gestureDetector.Value.AddGestures(e.NewItems?.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Replace:
					_gestureDetector.Value.RemoveGestures(e.OldItems?.OfType<IGestureRecognizer>());
					_gestureDetector.Value.AddGestures(e.NewItems?.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Remove:
					_gestureDetector.Value.RemoveGestures(e.OldItems?.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Reset:
					_gestureDetector.Value.Clear();
					break;
			}
		}
	}
}
