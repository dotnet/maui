#nullable disable
using System;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	[Obsolete("Scrolling is now handled by KeyboardAutoManagerScroll.")]
	internal class KeyboardInsetTracker : IDisposable
	{
		readonly Func<UIWindow> _fetchWindow;
		readonly Action<PointF> _setContentOffset;
		readonly Action<UIEdgeInsets> _setInsetAction;
		readonly UIScrollView _targetView;
		bool _disposed;
		UIEdgeInsets _currentInset;
		RectangleF _lastKeyboardRect;
		ShellScrollViewTracker _shellScrollViewTracker;


		public KeyboardInsetTracker(UIScrollView targetView, Func<UIWindow> fetchWindow, Action<UIEdgeInsets> setInsetAction)
			: this(targetView, fetchWindow, setInsetAction, null)
		{
		}

		public KeyboardInsetTracker(UIScrollView targetView, Func<UIWindow> fetchWindow, Action<UIEdgeInsets> setInsetAction, Action<PointF> setContentOffset)
			 : this(targetView, fetchWindow, setInsetAction, setContentOffset, null)
		{
		}

		public KeyboardInsetTracker(UIScrollView targetView, Func<UIWindow> fetchWindow, Action<UIEdgeInsets> setInsetAction, Action<PointF> setContentOffset, IPlatformViewHandler renderer)
		{
			_setContentOffset = setContentOffset;
			_targetView = targetView;
			_fetchWindow = fetchWindow;
			_setInsetAction = setInsetAction;
			Controls.Compatibility.Platform.iOS.KeyboardObserver.KeyboardWillShow += OnKeyboardShown;
			Controls.Compatibility.Platform.iOS.KeyboardObserver.KeyboardWillHide += OnKeyboardHidden;
			if (renderer != null)
				_shellScrollViewTracker = new ShellScrollViewTracker(renderer);
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			Controls.Compatibility.Platform.iOS.KeyboardObserver.KeyboardWillShow -= OnKeyboardShown;
			Controls.Compatibility.Platform.iOS.KeyboardObserver.KeyboardWillHide -= OnKeyboardHidden;

			_shellScrollViewTracker?.Dispose();
			_shellScrollViewTracker = null;
		}

		//This method allows us to update the insets if the Frame changes
		internal void UpdateInsets()
		{
			//being called from LayoutSubviews but keyboard wasn't shown yet
			if (_lastKeyboardRect.IsEmpty)
				return;

			var window = _fetchWindow();
			// Code left verbose to make its operation more obvious
			if (window == null)
			{
				// we are not currently displayed and can safely ignore this
				// most likely this renderer is on a page which is currently not displayed (e.g. in NavController)
				return;
			}

			var field = Controls.Compatibility.Platform.iOS.UIViewExtensions.FindFirstResponder(_targetView);

			//the view that is triggering the keyboard is not inside our UITableView?
			//if (field == null)
			//	return;

			var boundsSize = _targetView.Frame.Size;

			//since our keyboard frame is RVC CoordinateSpace, lets convert it to our targetView CoordinateSpace
			var rect = _targetView.Superview.ConvertRectFromView(_lastKeyboardRect, null);
			//let's see how much does it cover our target view
			var overlay = RectangleF.Intersect(rect, _targetView.Frame);

			_currentInset = _targetView.ContentInset;
			_setInsetAction(new UIEdgeInsets(0, 0, overlay.Height, 0));

			if (field is UITextView && _setContentOffset != null)
			{
				var keyboardTop = boundsSize.Height - overlay.Height;
				var fieldPosition = field.ConvertPointToView(field.Frame.Location, _targetView.Superview);
				var fieldBottom = fieldPosition.Y + field.Frame.Height;
				var offset = fieldBottom - keyboardTop;
				if (offset > 0)
					_setContentOffset(new PointF(0, offset));
			}
		}

		public void OnLayoutSubviews() => _shellScrollViewTracker?.OnLayoutSubviews();

		void OnKeyboardHidden(object sender, UIKeyboardEventArgs args)
		{
			if (_shellScrollViewTracker == null || !_shellScrollViewTracker.Reset())
				_setInsetAction(new UIEdgeInsets(0, 0, 0, 0));

			_lastKeyboardRect = RectangleF.Empty;
		}

		void OnKeyboardShown(object sender, UIKeyboardEventArgs args)
		{
			_lastKeyboardRect = args.FrameEnd;
			UpdateInsets();
		}
	}
}
