using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class ResignFirstResponderTouchGestureRecognizer : UITapGestureRecognizer
	{
		readonly WeakReference<UIView> _targetView;

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: UIViewSubclassTests.ResignFirstResponderTouchGestureRecognizer")]
		Token? _token;

		public ResignFirstResponderTouchGestureRecognizer(UIView targetView)
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

			_targetView = new(targetView);
		}

		void OnTapped()
		{
			if (_targetView.TryGetTarget(out var targetView) && targetView.IsFirstResponder)
				targetView.ResignFirstResponder();

			Disconnect();
		}

		internal void Disconnect()
		{
			if (_token != null)
				RemoveTarget(_token);

			_token = null;
		}

		static bool OnShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			foreach (UIView v in ViewAndSuperviewsOfView(touch.View))
			{
				if (v != null && (v is UITableView || v is UITableViewCell || v.CanBecomeFirstResponder))
					return false;
			}

			return true;
		}

		static IEnumerable<UIView> ViewAndSuperviewsOfView(UIView view)
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
