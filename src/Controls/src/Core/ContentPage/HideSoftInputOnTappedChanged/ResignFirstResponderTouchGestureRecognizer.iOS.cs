using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class ResignFirstResponderTouchGestureRecognizer : UITapGestureRecognizer
	{
		readonly WeakReference<UIView> _targetView;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: UIViewSubclassTests.ResignFirstResponderTouchGestureRecognizer")]
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

			_targetView = new(targetView);
		}

		void OnTapped()
		{
			if (_targetView.TryGetTarget(out var targetView) && targetView.IsFirstResponder)
				targetView.ResignFirstResponder();

			Disconnect();
		}

		void Disconnect()
		{
			if (_token != null)
				RemoveTarget(_token);

			if (_targetView.TryGetTarget(out var targetView) && targetView?.Window is UIWindow window)
				window.RemoveGestureRecognizer(this);

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
			while (view is not null)
			{
				yield return view;
				view = view.Superview!;
			}
		}


		static bool ConnectToPlatformEvents(UIView uiView)
		{
			if (uiView is UITextView textView)
			{
				textView.Started += OnEditingDidBegin;
				textView.Ended += OnEditingDidEnd;
				return true;
			}
			else if (uiView is UIControl uiControl)
			{
				uiControl.EditingDidBegin += OnEditingDidBegin;
				uiControl.EditingDidEnd += OnEditingDidEnd;
				return true;
			}

			return false;
		}


		static void DisconnectFromPlatformEvents(UIView uiView)
		{
			if (uiView is UITextView textView)
			{
				textView.Started -= OnEditingDidBegin;
				textView.Ended -= OnEditingDidEnd;
			}
			else if (uiView is UIControl uiControl)
			{
				uiControl.EditingDidBegin -= OnEditingDidBegin;
				uiControl.EditingDidEnd -= OnEditingDidEnd;
			}
		}

		internal static IDisposable? Update(UIView uiView)
		{
			if (uiView.Window is not UIWindow window)
			{
				DisconnectFromPlatformEvents(uiView);
				return null;
			}

			if (!ConnectToPlatformEvents(uiView))
				return null;

			if (uiView.IsFirstResponder)
				OnEditingDidBegin(uiView, EventArgs.Empty);

			var localWindow = window;
			var localControl = uiView;

			return new ActionDisposable(() =>
			{
				DisconnectFromPlatformEvents(localControl);
				Remove(localWindow);

				localWindow = null;
				localControl = null;
			});
		}

		static void OnEditingDidBegin(object? sender, EventArgs e)
		{
			if (sender is UIView view && view.Window is not null)
			{
				Remove(view.Window);
				var resignFirstResponder = new ResignFirstResponderTouchGestureRecognizer(view);
				view.Window.AddGestureRecognizer(resignFirstResponder);
				return;
			}
		}

		static void OnEditingDidEnd(object? sender, EventArgs e)
		{
			if (sender is UIView view)
			{
				Remove(view.Window);
			}
		}

		static void Remove(UIWindow? window)
		{
			var grs = window?.GestureRecognizers;
			if (grs is not null)
			{
				for (var i = grs.Length - 1; i >= 0; i--)
				{
					UIGestureRecognizer? gr = grs[i];
					if (gr is ResignFirstResponderTouchGestureRecognizer gesture)
					{
						gesture.Disconnect();
					}
				}
			}
		}
	}
}
