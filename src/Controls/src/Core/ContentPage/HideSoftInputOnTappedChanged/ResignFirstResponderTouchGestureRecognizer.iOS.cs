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
			while (view is not null)
			{
				yield return view;
				view = view.Superview;
			}
		}

		internal static void Update(UITextView textView, UIWindow? window)
		{
			if (window is not null)
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

		internal static void Update(
			UITextView textView,
			UIWindow? window,
			out IDisposable unwire)
		{
			if (window is not null)
			{
				Update(textView, window);
				unwire = new ActionDisposable(() => Update(textView, window));
			}
			else
			{
				Update(textView, window);
				unwire = new ActionDisposable(() => { });
			}
		}

		internal static void Update(UIControl platformControl, UIWindow? window)
		{
			if (window is not null)
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

		internal static void Update(
			UIControl platformControl,
			UIWindow? window,
			out IDisposable unwire)
		{
			if (window is not null)
			{
				Update(platformControl, window);
				unwire = new ActionDisposable(() => Update(platformControl, window));
			}
			else
			{
				Update(platformControl, window);
				unwire = new ActionDisposable(() => { });
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
				Remove(view.Window);
			}
		}

		static void Remove(UIWindow window)
		{
			if (window.GestureRecognizers != null)
			{
				for (var i = 0; i < window.GestureRecognizers.Length; i++)
				{
					UIGestureRecognizer? gr = window.GestureRecognizers[i];
					if (gr is ResignFirstResponderTouchGestureRecognizer)
					{
						window.RemoveGestureRecognizer(gr);
						return;
					}
				}
			}
		}
	}
}
