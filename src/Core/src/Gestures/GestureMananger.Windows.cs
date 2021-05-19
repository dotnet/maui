#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui
{
	public class GestureMananger : IGestureManager
	{
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;

		bool _isDisposed;
		IViewHandler? _handler;
		IView? _virtualView;
		FrameworkElement? _nativeView;

		public GestureMananger()
		{
			_collectionChangedHandler = OnGestureRecognizersCollectionChanged;
		}

		public void SetViewHandler(IViewHandler handler)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(null);

			_handler = handler ?? throw new ArgumentNullException(nameof(handler));

			_virtualView = _handler.VirtualView as IView;
			_nativeView = _handler.NativeView as FrameworkElement;

			if (_virtualView != null)
			{
				if (_virtualView is IView view)
				{
					var gestureRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
					gestureRecognizers.CollectionChanged += _collectionChangedHandler;
				}
			}

			UpdatingGestureRecognizers();
		}

		public void Dispose() 
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (!disposing)
				return;

			if (_virtualView != null)
			{
				if (_virtualView is IView view)
				{
					var oldRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
					oldRecognizers.CollectionChanged -= _collectionChangedHandler;
				}
			}

			if (_nativeView != null)
			{
				_nativeView.Tapped -= OnTap;
				_nativeView.DoubleTapped -= OnDoubleTap;
			}
		}

		void UpdatingGestureRecognizers()
		{
			if (_nativeView == null)
				return;

			IList<IGestureRecognizer>? gestures = _virtualView?.GestureRecognizers;

			if (gestures == null)
				return;

			var children = _virtualView?.GetChildElements(Point.Zero);
			IList<ITapGestureRecognizer>? childGestures = children?.GetChildGesturesFor<ITapGestureRecognizer>().ToList();

			if (gestures.GetGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1).Any()
				|| children?.GetChildGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1).Any() == true)
			{
				_nativeView.Tapped += OnTap;
			}
		
			if (gestures.GetGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2).Any()
				|| children?.GetChildGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2).Any() == true)
			{
				_nativeView.DoubleTapped += OnDoubleTap;
			}
		}

		void OnTap(object? sender, TappedRoutedEventArgs e)
		{
			if (_virtualView is not IView view)
				return;

			if (view == null)
				return;

			var tapPosition = e.GetPosition(_nativeView);
			var children = view.GetChildElements(new Point(tapPosition.X, tapPosition.Y));

			if (children != null)
				foreach (var recognizer in children.GetChildGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1))
				{
					recognizer.Tapped(view);
					e.Handled = true;
				}

			if (e.Handled)
				return;

			IEnumerable<ITapGestureRecognizer> tapGestures = view.GestureRecognizers.GetGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1);
			foreach (var recognizer in tapGestures)
			{
				recognizer.Tapped(view);
				e.Handled = true;
			}
		}

		void OnDoubleTap(object? sender, DoubleTappedRoutedEventArgs e)
		{
			if (_virtualView is not IView view)
				return;

			if (view == null)
				return;

			var tapPosition = e.GetPosition(_nativeView);
			var children = view.GetChildElements(new Point(tapPosition.X, tapPosition.Y));

			if (children != null)
				foreach (var recognizer in children.GetChildGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2))
				{
					recognizer.Tapped(view);
					e.Handled = true;
				}

			if (e.Handled)
				return;

			IEnumerable<ITapGestureRecognizer> doubleTapGestures = view.GestureRecognizers.GetGesturesFor<ITapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2);
			foreach (ITapGestureRecognizer recognizer in doubleTapGestures)
			{
				recognizer.Tapped(view);
				e.Handled = true;
			}
		}

		void OnGestureRecognizersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdatingGestureRecognizers();
		}
	}
}