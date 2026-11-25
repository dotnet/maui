using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AView = Android.Views.View;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Microsoft.Maui.Platform
{
	internal sealed class PickerClearButtonManager : IDisposable
	{
		readonly AppCompatEditText _view;
		readonly Action _clearSelection;
		readonly Action _refreshClearButton;
		readonly PickerClearButtonClickListener _clickListener;
		Drawable? _drawable;
		Drawable?[]? _originalDrawables;
		IPicker? _picker;
		AppCompatAlertDialog? _dialog;
		bool _visible;
		bool _trackingTouch;
		float _touchDownX;
		float _touchDownY;
		readonly int _touchSlop;

		internal PickerClearButtonManager(AppCompatEditText view, Action clearSelection, Action refreshClearButton)
		{
			_view = view;
			_clearSelection = clearSelection;
			_refreshClearButton = refreshClearButton;
			_clickListener = new PickerClearButtonClickListener(this);
			var context = view.Context ?? throw new InvalidOperationException("The picker clear-button manager requires a platform context.");
			_touchSlop = Math.Max(1, ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 0);
			_view.Touch += OnTouch;
		}

		internal void Update(IPicker picker)
		{
			_picker = picker;
			var visible = picker.ShouldShowClearButton();

			if (visible)
			{
				_drawable ??= ContextCompat.GetDrawable(_view.Context, Resource.Drawable.abc_ic_clear_material)?.Mutate();
				if (_drawable is null)
					throw new InvalidOperationException("The picker clear-button drawable could not be loaded.");

				if (!_visible)
					_originalDrawables = _view.GetCompoundDrawablesRelative();

				var density = _view.Resources?.DisplayMetrics?.Density ?? 1;
				// Keep the icon within the text line so toggling it cannot increase the field height.
				var size = Math.Max(1, (int)Math.Min(20 * density, _view.LineHeight));
				_drawable.SetBounds(0, 0, size, size);
				_view.UpdateClearButtonColor(picker.TextColor, _drawable);
				_view.SetCompoundDrawablesRelative(
					_originalDrawables?[0], _originalDrawables?[1], _drawable, _originalDrawables?[3]);
			}
			else if (_visible)
			{
				_view.SetCompoundDrawablesRelative(
					_originalDrawables?[0], _originalDrawables?[1], _originalDrawables?[2], _originalDrawables?[3]);
				_originalDrawables = null;
			}

			_visible = visible;

			if (_dialog?.GetButton((int)DialogButtonType.Neutral) is { } button)
			{
				button.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
				button.Enabled = picker.IsEnabled;
			}

			if (picker is IPickerClearButton { IsClearButtonVisible: true } &&
				_dialog?.ListView is { } list && list.CheckedItemPosition != picker.SelectedIndex)
			{
				list.ClearChoices();
				if (picker.SelectedIndex >= 0)
					list.SetItemChecked(picker.SelectedIndex, true);
				list.InvalidateViews();
			}
		}

		internal void SetDialog(AppCompatAlertDialog? dialog)
		{
			if (_dialog?.GetButton((int)DialogButtonType.Neutral) is { } previousButton)
				previousButton.SetOnClickListener(null);

			_dialog = dialog;

			// Replace AlertDialog's default listener so clearing does not dismiss the dialog.
			_dialog?.GetButton((int)DialogButtonType.Neutral)?.SetOnClickListener(_clickListener);
			if (_picker is not null)
				_refreshClearButton();
		}

		internal void ClearSelection()
		{
			if (_picker is not { IsEnabled: true } picker || !picker.ShouldShowClearButton())
				return;

			_clearSelection();
		}

		void OnTouch(object? sender, AView.TouchEventArgs e)
		{
			if (_picker is not { IsEnabled: true } picker ||
				picker is not IPickerClearButton { IsClearButtonVisible: true })
			{
				_trackingTouch = false;
				return;
			}

			e.Handled = false;
			if (!_visible)
			{
				_trackingTouch = false;
				return;
			}

			var motion = e.Event;
			if (motion is null)
			{
				_trackingTouch = false;
				return;
			}

			switch (motion.ActionMasked)
			{
				case MotionEventActions.Down:
					_trackingTouch = _visible && _picker is { IsEnabled: true } && IsInClearTarget(motion);
					if (_trackingTouch)
					{
						_touchDownX = motion.GetX();
						_touchDownY = motion.GetY();
					}
					break;
				case MotionEventActions.Move:
					if (_trackingTouch && HasExceededTouchSlop(motion))
						_trackingTouch = false;
					break;
				case MotionEventActions.Up:
					if (_trackingTouch)
					{
						_trackingTouch = false;
						if (!HasExceededTouchSlop(motion) && IsInClearTarget(motion))
						{
							e.Handled = true;
							ClearSelection();
						}
					}
					break;
				case MotionEventActions.Cancel:
				case MotionEventActions.PointerDown:
					_trackingTouch = false;
					break;
			}
		}

		bool HasExceededTouchSlop(MotionEvent motion) =>
			Math.Max(Math.Abs(motion.GetX() - _touchDownX), Math.Abs(motion.GetY() - _touchDownY)) >= _touchSlop;

		bool IsInClearTarget(MotionEvent motion)
		{
			var x = motion.GetX();
			var y = motion.GetY();
			if (x < 0 || x > _view.Width || y < 0 || y > _view.Height)
				return false;

			var targetWidth = Math.Max(_drawable?.IntrinsicWidth ?? 0,
				48 * (_view.Resources?.DisplayMetrics?.Density ?? 1));
			return _view.LayoutDirection == LayoutDirection.Rtl
				? x <= _view.PaddingLeft + targetWidth
				: x >= _view.Width - _view.PaddingRight - targetWidth;
		}

		public void Dispose()
		{
			_view.Touch -= OnTouch;
			if (_dialog?.GetButton((int)DialogButtonType.Neutral) is { } button)
			{
				button.SetOnClickListener(null);
			}
			_dialog = null;
			if (_visible)
			{
				_view.SetCompoundDrawablesRelative(
					_originalDrawables?[0], _originalDrawables?[1], _originalDrawables?[2], _originalDrawables?[3]);
			}
			_picker = null;
			_drawable?.Dispose();
			_drawable = null;
			_clickListener.Dispose();
		}

	}

	internal sealed class PickerClearButtonClickListener : Java.Lang.Object, AView.IOnClickListener
	{
		readonly WeakReference<PickerClearButtonManager> _manager;

		public PickerClearButtonClickListener(PickerClearButtonManager manager)
		{
			_manager = new WeakReference<PickerClearButtonManager>(manager);
		}

		public void OnClick(AView? view)
		{
			if (_manager.TryGetTarget(out var manager))
				manager.ClearSelection();
		}
	}
}
