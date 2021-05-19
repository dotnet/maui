#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UIKit;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class GestureManager : IGestureManager
	{
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
		readonly Dictionary<IGestureRecognizer, UIGestureRecognizer> _gestureRecognizers;

		bool _isDisposed;
		IViewHandler? _handler; 
		IView? _virtualView;
		UIView? _nativeView;
		UITouchEventArgs? _shouldReceiveTouch;

		public GestureManager()
		{
			_collectionChangedHandler = OnGestureRecognizersCollectionChanged;
			_gestureRecognizers = new Dictionary<IGestureRecognizer, UIGestureRecognizer>();
		}

		internal ObservableCollection<IGestureRecognizer>? VirtualViewGestureRecognizers =>
			_virtualView?.CompositeGestureRecognizers as ObservableCollection<IGestureRecognizer>;

		public void SetViewHandler(IViewHandler handler)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(null);

			_handler = handler ?? throw new ArgumentNullException(nameof(handler));

			_virtualView = _handler.VirtualView as IView;
			_nativeView = _handler.NativeView as UIView;

			if (VirtualViewGestureRecognizers != null)
				VirtualViewGestureRecognizers.CollectionChanged += _collectionChangedHandler;

			LoadRecognizers();
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (_gestureRecognizers != null)
			{
				foreach (var kvp in _gestureRecognizers)
				{
					_nativeView?.RemoveGestureRecognizer(kvp.Value);

					kvp.Value.ShouldReceiveTouch = null;

					kvp.Value.Dispose();
				}

				_gestureRecognizers.Clear();
			}

			if (VirtualViewGestureRecognizers != null)
				VirtualViewGestureRecognizers.CollectionChanged -= _collectionChangedHandler;

			_nativeView = null;
		}
				
		static IList<IGestureView>? GetChildGestures(
			UIGestureRecognizer sender,
			WeakReference weakEventTracker, WeakReference weakRecognizer, GestureManager? gestureManager, IView? view)
		{
			if (weakEventTracker == null)
				return null;

			if (!weakRecognizer.IsAlive)
				return null;

			if (gestureManager == null || gestureManager._isDisposed || view == null)
				return null;

			var nativeView = (UIView?)gestureManager._handler?.NativeView;

			if (nativeView == null)
				return null;

			var originPoint = sender.LocationInView(nativeView);
			var childGestures = view.GetChildElements(new Point(originPoint.X, originPoint.Y));

			return childGestures;
		}

		Action<UITapGestureRecognizer> CreateRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer, ITapGestureRecognizer clickRecognizer)
		{
			return new Action<UITapGestureRecognizer>((sender) =>
			{
				GestureManager? eventTracker = weakEventTracker.Target as GestureManager;
				IView? virtualView = (IView?)eventTracker?._handler?.VirtualView;

				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, eventTracker, virtualView);

				if (childGestures?.GetChildGesturesFor<ITapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)sender.NumberOfTapsRequired).Count() > 0)
					return;

				if (weakRecognizer.Target is ITapGestureRecognizer tapGestureRecognizer && virtualView != null)
					tapGestureRecognizer.Tapped(virtualView);
			});
		}

		Action<UITapGestureRecognizer> CreateChildRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer)
		{
			return new Action<UITapGestureRecognizer>((sender) =>
			{
				GestureManager? gestureManager = weakEventTracker.Target as GestureManager;
				IView? virtualView = (IView?)gestureManager?._handler?.VirtualView;

				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, gestureManager, virtualView);

				var recognizers = childGestures?.GetChildGesturesFor<ITapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)sender.NumberOfTapsRequired);

				if (recognizers == null)
					return;

				var tapGestureRecognizer = (weakRecognizer.Target as IChildGestureRecognizer)?.GestureRecognizer as ITapGestureRecognizer;

				foreach (var item in recognizers)
					if (item == tapGestureRecognizer && virtualView != null)
						tapGestureRecognizer?.Tapped(virtualView);
			});
		}

		protected virtual UIGestureRecognizer? GetNativeRecognizer(IGestureRecognizer recognizer)
		{
			if (recognizer == null)
				return null;

			var weakRecognizer = new WeakReference(recognizer);
			var weakEventTracker = new WeakReference(this);

			if (recognizer is ITapGestureRecognizer tapRecognizer)
			{
				var returnAction = CreateRecognizerHandler(weakEventTracker, weakRecognizer, tapRecognizer);
				var nativeRecognizer = CreateTapRecognizer(tapRecognizer.NumberOfTapsRequired, returnAction);

				return nativeRecognizer;
			}

			if (recognizer is IChildGestureRecognizer childRecognizer)
			{
				if (childRecognizer.GestureRecognizer is ITapGestureRecognizer tapChildRecognizer)
				{
					var returnAction = CreateChildRecognizerHandler(weakEventTracker, weakRecognizer);
					var nativeRecognizer = CreateTapRecognizer(tapChildRecognizer.NumberOfTapsRequired, returnAction);

					return nativeRecognizer;
				}
			}

			return null;
		}

		UITapGestureRecognizer CreateTapRecognizer(int numTaps, Action<UITapGestureRecognizer> action, int numFingers = 1)
		{
			var result = new UITapGestureRecognizer(action)
			{
				NumberOfTouchesRequired = (uint)numFingers,
				NumberOfTapsRequired = (uint)numTaps,
				ShouldRecognizeSimultaneously = ShouldRecognizeTapsTogether
			};

			return result;
		}

		static bool ShouldRecognizeTapsTogether(UIGestureRecognizer gesture, UIGestureRecognizer other)
		{
			// If multiple tap gestures are potentially firing (because multiple tap gesture recognizers have been
			// added to the XF Element), we want to allow them to fire simultaneously if they have the same number
			// of taps and touches

			if (gesture is not UITapGestureRecognizer tap)
			{
				return false;
			}

			if (other is not UITapGestureRecognizer otherTap)
			{
				return false;
			}

			if (!Equals(tap.View, otherTap.View))
			{
				return false;
			}

			if (tap.NumberOfTapsRequired != otherTap.NumberOfTapsRequired)
			{
				return false;
			}

			if (tap.NumberOfTouchesRequired != otherTap.NumberOfTouchesRequired)
			{
				return false;
			}

			return true;
		}

		void LoadRecognizers()
		{
			if (VirtualViewGestureRecognizers == null)
				return;

			if (_shouldReceiveTouch == null)
			{
				// Cache this so we don't create a new UITouchEventArgs instance for every recognizer
				_shouldReceiveTouch = ShouldReceiveTouch;
			}

			for (int i = 0; i < VirtualViewGestureRecognizers.Count; i++)
			{
				IGestureRecognizer recognizer = VirtualViewGestureRecognizers[i];
				if (_gestureRecognizers.ContainsKey(recognizer))
					continue;

				var nativeRecognizer = GetNativeRecognizer(recognizer);
				if (nativeRecognizer != null && _nativeView != null)
				{
					nativeRecognizer.ShouldReceiveTouch = _shouldReceiveTouch;

					_nativeView.AddGestureRecognizer(nativeRecognizer);

					_gestureRecognizers[recognizer] = nativeRecognizer;
				}
			}

			var toRemove = _gestureRecognizers.Keys.Where(key => !VirtualViewGestureRecognizers.Contains(key)).ToArray();

			for (int i = 0; i < toRemove.Length; i++)
			{
				IGestureRecognizer gestureRecognizer = toRemove[i];
				var uiRecognizer = _gestureRecognizers[gestureRecognizer];
				_gestureRecognizers.Remove(gestureRecognizer);

				_nativeView?.RemoveGestureRecognizer(uiRecognizer);
				uiRecognizer.Dispose();
			}
		}

		bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			if (touch.View is IViewHandler)
			{
				return true;
			}

			// If the touch is coming from the UIView our renderer is wrapping (e.g., if it's  
			// wrapping a UIView which already has a gesture recognizer), then we should let it through
			// (This goes for children of that control as well)
			if (_handler?.NativeView == null)
			{
				return false;
			}

			var nativeView = (UIView?)_handler.NativeView;

			if (nativeView == null)
			{
				return false;
			}

			if (touch.View.IsDescendantOfView(nativeView) &&
				(touch.View.GestureRecognizers?.Length > 0 || nativeView.GestureRecognizers?.Length > 0))
			{
				return true;
			}

			return false;
		}

		void OnGestureRecognizersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			LoadRecognizers();
		}
	}
}