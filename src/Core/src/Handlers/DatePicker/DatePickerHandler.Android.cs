using System;
using Android.App;
using Android.Views;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		DatePickerDialog? _dialog;
		(int year, int month, int day) _selectedDate;

		protected override MauiDatePicker CreatePlatformView()
		{
			var mauiDatePicker = new MauiDatePicker(Context)
			{
				ShowPicker = ShowPickerDialog,
				HidePicker = HidePickerDialog
			};

			var date = VirtualView?.Date;

			if (date != null)
				_dialog = CreateDatePickerDialog(date.Value.Year, date.Value.Month, date.Value.Day);

			return mauiDatePicker;
		}

		internal DatePickerDialog? DatePickerDialog { get { return _dialog; } }

		protected override void ConnectHandler(MauiDatePicker platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow += OnViewDetachedFromWindow;

			if (platformView.IsAttachedToWindow)
				OnViewAttachedToWindow();
		}

		void OnViewDetachedFromWindow(object? sender = null, View.ViewDetachedFromWindowEventArgs? e = null)
		{
			// I tested and this is called when an activity is destroyed
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
		}

		void OnViewAttachedToWindow(object? sender = null, View.ViewAttachedToWindowEventArgs? e = null)
		{
			DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		}

		protected override void DisconnectHandler(MauiDatePicker platformView)
		{
			if (_dialog != null)
			{
				_dialog.DismissEvent -= OnDismissEvent;
				_dialog.ShowEvent -= OnShowEvent;
				if (OperatingSystem.IsAndroidVersionAtLeast(24))
					_dialog.DateSet -= OnDateSet;

				_dialog.Hide();
				_dialog.Dispose();
				_dialog = null;
			}

			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
			OnViewDetachedFromWindow();

			base.DisconnectHandler(platformView);
		}

		protected virtual DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
		{
			var dialog = new DatePickerDialog(Context!, OnDateSet, year, month, day);
			return dialog;
		}

		public static partial void MapBackground(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateBackground(datePicker);
		}

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateDate(datePicker);
		}

		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMinimumDate(datePicker, platformHandler._dialog);
		}

		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMaximumDate(datePicker, platformHandler._dialog);
		}

		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(datePicker);
		}

		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateTextColor(datePicker);
		}

		void ShowPickerDialog()
		{
			if (VirtualView == null)
				return;

			if (_dialog != null && _dialog.IsShowing)
				return;

			VirtualView.IsFocused = true;
			var date = VirtualView.Date;
			ShowPickerDialog(date.Year, date.Month - 1, date.Day);
		}

		void ShowPickerDialog(int year, int month, int day)
		{
			if (_dialog == null)
				_dialog = CreateDatePickerDialog(year, month, day);
			else
			{
				_selectedDate = (year, month, day);
				_dialog.ShowEvent += OnShowEvent;
				_dialog.DismissEvent += OnDismissEvent;
			}

			_dialog.Show();
		}

		void OnDismissEvent(object? sender, EventArgs e)
		{
			if (VirtualView != null)
			{
				VirtualView.IsFocused = false;
			}

			if (_dialog != null)
			{
				_dialog.DismissEvent -= OnDismissEvent;
			}
		}

		void OnShowEvent(object? sender, EventArgs e)
		{
			_dialog!.UpdateDate(_selectedDate.year, _selectedDate.month, _selectedDate.day);
			_dialog.ShowEvent -= OnShowEvent;
		}

		void OnDateSet(object? obj, DatePickerDialog.DateSetEventArgs e)
		{
			if (VirtualView != null)
			{
				VirtualView.Date = e.Date;
				VirtualView.IsFocused = false;
			}
		}

		void HidePickerDialog()
		{
			_dialog?.Hide();
		}

		void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
		{
			DatePickerDialog? currentDialog = _dialog;

			if (currentDialog != null && currentDialog.IsShowing)
			{
				currentDialog.Dismiss();

				ShowPickerDialog(currentDialog.DatePicker.Year, currentDialog.DatePicker.Month, currentDialog.DatePicker.DayOfMonth);
			}
		}
	}
}
