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
		readonly ClearClickListener _clickListener;
		Drawable? _drawable;
		Drawable?[]? _originalDrawables;
		IPicker? _picker;
		AppCompatAlertDialog? _dialog;
		bool _visible;
		bool _trackingTouch;

		internal PickerClearButtonManager(AppCompatEditText view, Action clearSelection)
		{
			_view = view;
			_clearSelection = clearSelection;
			_clickListener = new ClearClickListener(this);
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

				_view.UpdateClearButtonColor(picker.TextColor, _drawable);
				_view.SetCompoundDrawablesRelativeWithIntrinsicBounds(
					_originalDrawables?[0], _originalDrawables?[1], _drawable, _originalDrawables?[3]);
			}
			else if (_visible)
			{
				_view.SetCompoundDrawablesRelativeWithIntrinsicBounds(
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
			if (_picker is { } picker)
				Update(picker);
		}

		void ClearSelection()
		{
			if (_picker is not { IsEnabled: true } picker || !picker.ShouldShowClearButton())
				return;

			_clearSelection();
		}

		void OnTouch(object? sender, AView.TouchEventArgs e)
		{
			e.Handled = false;
			var motion = e.Event;
			if (motion is null)
				return;

			if (motion.ActionMasked == MotionEventActions.Down)
			{
				_trackingTouch = _visible && _picker is { IsEnabled: true } && IsInClearTarget(motion);
			}

			if (!_trackingTouch)
			{
				e.Handled = false;
				return;
			}

			e.Handled = true;
			if (motion.ActionMasked == MotionEventActions.Up)
			{
				_trackingTouch = false;
				if (IsInClearTarget(motion))
					ClearSelection();
			}
			else if (motion.ActionMasked == MotionEventActions.Cancel)
			{
				_trackingTouch = false;
			}
		}

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
			SetDialog(null);
			if (_visible)
			{
				_view.SetCompoundDrawablesRelativeWithIntrinsicBounds(
					_originalDrawables?[0], _originalDrawables?[1], _originalDrawables?[2], _originalDrawables?[3]);
			}
			_picker = null;
			_drawable?.Dispose();
			_drawable = null;
			_clickListener.Dispose();
		}

		sealed class ClearClickListener(PickerClearButtonManager manager) : Java.Lang.Object, AView.IOnClickListener
		{
			readonly WeakReference<PickerClearButtonManager> _manager = new(manager);

			public void OnClick(AView? view)
			{
				if (_manager.TryGetTarget(out var manager))
					manager.ClearSelection();
			}
		}
	}
}
