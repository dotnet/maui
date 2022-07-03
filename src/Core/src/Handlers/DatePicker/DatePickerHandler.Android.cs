using System;
using Android.App;
using Android.Views;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		DatePickerDialog? _dialog;

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
			//platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
			//platformView.ViewDetachedFromWindow += OnViewDetachedFromWindow;

			//if (platformView.IsAttachedToWindow)
			//	DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		}

		void OnViewDetachedFromWindow(object? sender, View.ViewDetachedFromWindowEventArgs e)
		{
			// I tested and this is called when an activity is destroyed
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
		}

		void OnViewAttachedToWindow(object? sender, View.ViewAttachedToWindowEventArgs e)
		{
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
			DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		}

		protected override void DisconnectHandler(MauiDatePicker platformView)
		{
			if (_dialog != null)
			{
				_dialog.Hide();
				_dialog.Dispose();
				_dialog = null;
			}

			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;

			base.DisconnectHandler(platformView);
		}

		protected virtual DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
		{
			var dialog = new DatePickerDialog(Context!, (o, e) =>
			{
				if (VirtualView != null)
				{
					VirtualView.Date = e.Date;
				}
			}, year, month, day);

			return dialog;
		}

		// This is a Android-specific mapping
		public static void MapBackground(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateBackground(datePicker);
		}

		public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateDate(datePicker);
		}

		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMinimumDate(datePicker, platformHandler._dialog);
		}

		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMaximumDate(datePicker, platformHandler._dialog);
		}

		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(datePicker);
		}

		public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
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

			var date = VirtualView.Date;
			ShowPickerDialog(date.Year, date.Month - 1, date.Day);
		}

		void ShowPickerDialog(int year, int month, int day)
		{
			if (_dialog == null)
				_dialog = CreateDatePickerDialog(year, month, day);
			else
			{
				EventHandler? setDateLater = null;
				setDateLater = (sender, e) => { _dialog!.UpdateDate(year, month, day); _dialog.ShowEvent -= setDateLater; };
				_dialog.ShowEvent += setDateLater;
			}

			_dialog.Show();
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