using System;
using Android.App;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		DatePickerDialog? _dialog;

		internal DatePickerDialog? DatePickerDialog => _dialog;

		protected override MauiDatePicker CreatePlatformView()
		{
			var mauiDatePicker = new MauiDatePicker(Context);

			var date = VirtualView?.Date;

			if (date != null)
			{
				_dialog = CreateDatePickerDialog(date.Value.Year, date.Value.Month - 1, date.Value.Day);
			}

			return mauiDatePicker;
		}

		protected override void ConnectHandler(MauiDatePicker platformView)
		{
			base.ConnectHandler(platformView);

			platformView.ShowPicker = ShowPickerDialog;
			platformView.HidePicker = HidePickerDialog;

			platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow += OnViewDetachedFromWindow;

			if (platformView.IsAttachedToWindow)
			{
				OnViewAttachedToWindow();
			}
		}

		void OnViewDetachedFromWindow(object? sender = null, View.ViewDetachedFromWindowEventArgs? e = null)
		{
			// This is called when an activity is destroyed
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
				_dialog.Dismiss();
				_dialog = null;
			}

			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;

			platformView.ShowPicker = null;
			platformView.HidePicker = null;

			OnViewDetachedFromWindow();

			base.DisconnectHandler(platformView);
		}

		protected virtual DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
		{
			var dialog = new DatePickerDialog(Context!, (o, e) =>
			{
				if (VirtualView is not null)
				{
					VirtualView.Date = e.Date;
				}
			}, year, month, day);

			dialog.DismissEvent += OnDialogDismiss;

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

		internal static partial void MapIsOpen(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler.IsConnected() && handler is DatePickerHandler platformHandler)
			{
				if (datePicker.IsOpen)
					platformHandler.ShowPickerDialog(datePicker.Date);
				else
					platformHandler.HidePickerDialog();
			}
		}

		void ShowPickerDialog()
		{
			if (VirtualView is null)
			{
				return;
			}

			if (_dialog is not null && _dialog.IsShowing)
			{
				return;
			}

			var date = VirtualView.Date;
			ShowPickerDialog(date);
			VirtualView.IsOpen = true;
		}

		void ShowPickerDialog(DateTime? date)
		{
			var year = date?.Year ?? DateTime.Today.Year;
			var month = (date?.Month ?? DateTime.Today.Month) - 1;
			var day = date?.Day ?? DateTime.Today.Day;

			if (_dialog is null)
			{
				_dialog = CreateDatePickerDialog(year, month, day);
			}
			else
			{
				EventHandler? setDateLater = null;
				setDateLater = (sender, e) => { _dialog!.UpdateDate(year, month, day); _dialog.ShowEvent -= setDateLater; };
				_dialog.ShowEvent += setDateLater;
				_dialog.DismissEvent += OnDialogDismiss;
			}

			_dialog.Show();
		}

		void HidePickerDialog()
		{
			if (_dialog != null)
			{
				_dialog.DismissEvent -= OnDialogDismiss;
				_dialog.Hide();
			}

			VirtualView.IsOpen = false;
		}

		void OnDialogDismiss(object? sender, EventArgs e)
		{
			HidePickerDialog();
		}

		void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
		{
			DatePickerDialog? currentDialog = _dialog;

			if (currentDialog is not null && currentDialog.IsShowing)
			{
				currentDialog.Dismiss();

				ShowPickerDialog(currentDialog.DatePicker.DateTime);
			}
		}
	}
}