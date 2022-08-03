#nullable enable

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Platform
{
	class GestureManager : IDisposable
	{
		IViewHandler? _handler;
		GestureDetector? _gestureDetector;
		bool _disposed = false;

		protected virtual VisualElement? Element => _handler?.VirtualView as VisualElement;

		public GestureManager(IViewHandler handler)
		{
			_handler = handler;
			_gestureDetector = null;
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
					_gestureDetector?.Clear();
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
						_gestureDetector = new GestureDetector(_handler);
						_gestureDetector.AddGestures(ov.GestureRecognizers);
					}
				}
				newElement.PropertyChanged += OnElementPropertyChanged;
			}

			UpdateInputTransparent();
			UpdateIsEnabled();
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

		public void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				SetupElement(Element, null);
				if (_gestureDetector != null)
				{
					_gestureDetector.Dispose();
					_gestureDetector = null;
				}
				_handler = null;
			}
		}

		void UpdateInputTransparent()
		{
			if (Element != null && _gestureDetector != null)
			{
				_gestureDetector.InputTransparent = Element.InputTransparent;
			}
		}

		void UpdateIsEnabled()
		{
			if (Element != null && _gestureDetector != null)
			{
				_gestureDetector.IsEnabled = Element.IsEnabled;
			}
		}

		void OnGestureRecognizerCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (_gestureDetector == null)
			{
				_gestureDetector = new GestureDetector(_handler);
			}

			// Gestures will be registered/unregistered according to changes in the GestureRecognizers list
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					_gestureDetector.AddGestures(e.NewItems?.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Replace:
					_gestureDetector.RemoveGestures(e.OldItems?.OfType<IGestureRecognizer>());
					_gestureDetector.AddGestures(e.NewItems?.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Remove:
					_gestureDetector.RemoveGestures(e.OldItems?.OfType<IGestureRecognizer>());
					break;

				case NotifyCollectionChangedAction.Reset:
					_gestureDetector.Clear();
					break;
			}
		}
	}
}
