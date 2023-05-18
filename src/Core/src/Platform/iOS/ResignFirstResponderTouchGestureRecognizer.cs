using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class ResignFirstResponderTouchGestureRecognizer : UITapGestureRecognizer
	{
		UIView? _targetView;
		Token? _token;

		public ResignFirstResponderTouchGestureRecognizer(UIView targetView) :
			base()
		{
			ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true;
			ShouldReceiveTouch = OnShouldReceiveTouch;
			CancelsTouchesInView = false;
			DelaysTouchesEnded = false;
			DelaysTouchesBegan = false;

			_token = AddTarget((a) =>
			{
				if (a is ResignFirstResponderTouchGestureRecognizer gr && gr.State == UIGestureRecognizerState.Ended)
				{
					gr.OnTapped();
				}
			});

			_targetView = targetView;
		}

		void OnTapped()
		{
			if (_targetView?.IsFirstResponder == true)
				_targetView?.ResignFirstResponder();

			Disconnect();
		}

		internal void Disconnect()
		{
			if (_token != null)
				RemoveTarget(_token);

			_token = null;
			_targetView = null;
		}

		bool OnShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			foreach (UIView v in ViewAndSuperviewsOfView(touch.View))
			{
				if (v != null && (v is UITableView || v is UITableViewCell || v.CanBecomeFirstResponder))
					return false;
			}

			return true;
		}

		IEnumerable<UIView> ViewAndSuperviewsOfView(UIView view)
		{
			while (view != null)
			{
				yield return view;
				view = view.Superview;
			}
		}

		internal static void Update(UITextView textView, UIWindow? window)
		{
			if (window != null)
			{
				textView.Started += OnEditingDidBegin;
				textView.Ended += OnEditingDidEnd;
			}
			else
			{
				textView.Started -= OnEditingDidBegin;
				textView.Ended -= OnEditingDidEnd;
			}
		}

		internal static void Update(UIControl platformControl, UIWindow? window)
		{
			if (window != null)
			{
				platformControl.EditingDidBegin += OnEditingDidBegin;
				platformControl.EditingDidEnd += OnEditingDidEnd;
			}
			else
			{
				platformControl.EditingDidBegin -= OnEditingDidBegin;
				platformControl.EditingDidEnd -= OnEditingDidEnd;
			}
		}

		static void OnEditingDidBegin(object? sender, EventArgs e)
		{
			if (sender is UIView view && view.Window != null)
			{
				var resignFirstResponder = new ResignFirstResponderTouchGestureRecognizer(view);
				view.Window.AddGestureRecognizer(resignFirstResponder);
				return;
			}
		}

		static void OnEditingDidEnd(object? sender, EventArgs e)
		{
			if (sender is UIView view && view.Window?.GestureRecognizers != null)
			{
				for (var i = 0; i < view.Window.GestureRecognizers.Length; i++)
				{
					UIGestureRecognizer? gr = view.Window.GestureRecognizers[i];
					if (gr is ResignFirstResponderTouchGestureRecognizer)
					{
						view.Window.RemoveGestureRecognizer(gr);
						return;
					}
				}
			}
		}
	}
}
