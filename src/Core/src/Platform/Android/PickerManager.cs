using System;
using System.Collections.Generic;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	internal static class PickerManager
	{
		const float DragDirectionRatio = 1.2f;
		static readonly System.Runtime.CompilerServices.ConditionalWeakTable<EditText, PickerTouchListener> TouchListeners = new();

		public static void Init(EditText editText)
		{
			editText.Focusable = true;
			editText.FocusableInTouchMode = false;
			editText.Clickable = true;
			editText.SetHorizontallyScrolling(true);
			editText.SetSingleLine(true);
			editText.SetTextIsSelectable(false);
			editText.LongClickable = false;
			editText.SetCursorVisible(false);

			// InputType needs to be set before setting KeyListener
			editText.InputType = InputTypes.Null;
			editText.KeyListener = null;

			SetTouchListener(editText);
		}

		public static void Dispose(EditText editText)
		{
			RemoveTouchListener(editText);
			editText.SetOnClickListener(null);
		}

		static void SetTouchListener(EditText editText)
		{
			RemoveTouchListener(editText);

			var listener = new PickerTouchListener();
			TouchListeners.Add(editText, listener);
			editText.SetOnTouchListener(listener);
		}

		static void RemoveTouchListener(EditText editText)
		{
			editText.SetOnTouchListener(null);

			if (TouchListeners.TryGetValue(editText, out var listener))
			{
				TouchListeners.Remove(editText);
				listener.Dispose();
			}
		}

		static bool OnTouchEvent(
			EditText editText,
			MotionEvent? e,
			ref float startX,
			ref float startY,
			ref float lastX,
			ref bool isHorizontalScrolling)
		{
			if (e is null)
			{
				return false;
			}

			if (!CanScrollHorizontally(editText))
			{
				if (isHorizontalScrolling)
				{
					editText.Parent?.RequestDisallowInterceptTouchEvent(false);
				}

				isHorizontalScrolling = false;
				return false;
			}

			switch (e.ActionMasked)
			{
				case MotionEventActions.Down:
					startX = lastX = e.GetX();
					startY = e.GetY();
					isHorizontalScrolling = false;
					return false;

				case MotionEventActions.Move:
					var currentX = e.GetX();
					var totalX = Math.Abs(currentX - startX);
					var totalY = Math.Abs(e.GetY() - startY);

					if (!isHorizontalScrolling)
					{
						var context = editText.Context;

						if (context is null)
						{
							return false;
						}

						var touchSlop = ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 0;

						if (totalX <= touchSlop || totalX <= totalY * DragDirectionRatio)
						{
							return false;
						}

						isHorizontalScrolling = true;
						editText.Parent?.RequestDisallowInterceptTouchEvent(true);
					}

					var deltaX = lastX - currentX;
					lastX = currentX;
					ScrollHorizontally(editText, deltaX);
					return true;

				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
					var wasHorizontalScrolling = isHorizontalScrolling;
					isHorizontalScrolling = false;

					if (wasHorizontalScrolling)
					{
						editText.Parent?.RequestDisallowInterceptTouchEvent(false);
					}

					return wasHorizontalScrolling;
			}

			return false;
		}

		static bool CanScrollHorizontally(EditText editText)
		{
			return editText.Text?.Length > 0 &&
				editText.Layout is not null &&
				GetMaxHorizontalScroll(editText) > 0;
		}

		static void ScrollHorizontally(EditText editText, float deltaX)
		{
			var maxScroll = GetMaxHorizontalScroll(editText);
			var newScrollX = Math.Clamp(editText.ScrollX + (int)Math.Round(deltaX), 0, maxScroll);

			if (newScrollX != editText.ScrollX)
			{
				editText.ScrollTo(newScrollX, editText.ScrollY);
			}
		}

		static int GetMaxHorizontalScroll(EditText editText)
		{
			var layout = editText.Layout;

			if (layout is null || editText.LineCount == 0)
			{
				return 0;
			}

			var availableWidth = editText.Width - editText.TotalPaddingLeft - editText.TotalPaddingRight;

			if (availableWidth <= 0)
			{
				return 0;
			}

			return Math.Max(0, (int)Math.Ceiling(layout.GetLineWidth(0)) - availableWidth);
		}

		sealed class PickerTouchListener : Java.Lang.Object, View.IOnTouchListener
		{
			float _startX;
			float _startY;
			float _lastX;
			bool _isHorizontalScrolling;

			public bool OnTouch(View? v, MotionEvent? e)
			{
				if (v is not EditText editText)
				{
					return false;
				}

				return OnTouchEvent(editText, e, ref _startX, ref _startY, ref _lastX, ref _isHorizontalScrolling);
			}
		}
	}
}
