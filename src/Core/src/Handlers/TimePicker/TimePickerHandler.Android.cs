using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text.Format;
using DateFormat = Android.Text.Format.DateFormat;
using Android.Views;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		MauiTimePicker? _timePicker;
		TimePickerDialog? _dialog;

		protected override MauiTimePicker CreatePlatformView()
		{
			_timePicker = new MauiTimePicker(Context);

			return _timePicker;
		}

		protected override void ConnectHandler(MauiTimePicker platformView)
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
			// Called when an activity is destroyed or view is detached
			DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
		}

		void OnViewAttachedToWindow(object? sender = null, View.ViewAttachedToWindowEventArgs? e = null)
		{
			DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		}

		protected override void DisconnectHandler(MauiTimePicker platformView)
		{
			if (_dialog != null)
			{
				_dialog.DismissEvent -= OnDialogDismiss;
				_dialog.Dismiss();
				_dialog = null;
			}

			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
			OnViewDetachedFromWindow();

			platformView.ShowPicker = null;
			platformView.HidePicker = null;

			base.DisconnectHandler(platformView);
		}

		protected virtual TimePickerDialog CreateTimePickerDialog(int hour, int minute)
		{
			void onTimeSetCallback(object? obj, TimePickerDialog.TimeSetEventArgs args)
			{
				if (VirtualView == null || PlatformView == null)
					return;

				VirtualView.Time = new TimeSpan(args.HourOfDay, args.Minute, 0);
				VirtualView.IsFocused = false;

				if (_dialog != null)
				{
					_dialog = null;
				}
			}

			var dialog = new TimePickerDialog(Context!, onTimeSetCallback, hour, minute, Use24HourView);
			dialog.DismissEvent += OnDialogDismiss;

			return dialog;
		}

		// This is a Android-specific mapping
		public static void MapBackground(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateBackground(timePicker);
		}

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFormat(timePicker);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker);
		}

		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(timePicker, fontManager);
		}

		// Make it public in .NET 11.
		internal static void MapFlowDirection(ITimePickerHandler handler, ITimePicker timePicker)
		{
			if (handler.PlatformView is not null)
			{
				handler.PlatformView.UpdateFlowDirection(timePicker);

				// For 12-hour format, also apply text alignment to handle AM/PM positioning
				// For 24-hour format, UpdateFlowDirection alone is sufficient
				if (handler is TimePickerHandler timePickerHandler && !timePickerHandler.Use24HourView)
				{
					handler.PlatformView.UpdateTextAlignment(timePicker);
				}
			}
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker);
		}

		internal static void MapIsOpen(ITimePickerHandler handler, ITimePicker timePicker)
		{
			if (handler.IsConnected() && handler is TimePickerHandler timePickerHandler)
			{
				if (timePicker.IsOpen)
					timePickerHandler.ShowPickerDialog();
				else
					timePickerHandler.HidePickerDialog();
			}
		}

		void ShowPickerDialog()
		{
			if (VirtualView is null)
			{
				return;
			}

			ShowPickerDialog(VirtualView.Time);
		}

		// This overload is here so we can pass in the current values from the dialog 
		// on an orientation change (so that orientation changes don't cause the user's date selection progress
		// to be lost). Not useful until we have orientation changed events.
		void ShowPickerDialog(TimeSpan? time)
		{
			if (_dialog is not null && _dialog.IsShowing)
			{
				return;
			}

			var hour = time?.Hours ?? 0;
			var minute = time?.Minutes ?? 0;

			_dialog = CreateTimePickerDialog(hour, minute);
			_dialog.Show();

			if (VirtualView is not null)
			{
				VirtualView.IsOpen = true;
			}
		}

		void HidePickerDialog()
		{
			if (_dialog is not null)
			{
				_dialog.DismissEvent -= OnDialogDismiss;
				_dialog.Hide();
			}

			_dialog = null;
			VirtualView.IsOpen = false;
		}

		void OnDialogDismiss(object? sender, EventArgs e)
		{
			HidePickerDialog();
		}

		// "HH" (uppercase) is the .NET 24-hour specifier; "hh" (lowercase) is 12-hour.
		// Case-sensitive Ordinal comparison is required to distinguish between them.
		internal static bool IsCustom24HourFormat(string? format) =>
			!string.IsNullOrEmpty(format) && format.Contains("HH", StringComparison.Ordinal);

		bool Use24HourView
		{
			get
			{
				if (VirtualView is null || string.IsNullOrEmpty(VirtualView.Format))
					return false;

				if (VirtualView.Format == "t")
					return DateFormat.Is24HourFormat(PlatformView?.Context);

				return IsCustom24HourFormat(VirtualView.Format);
			}
		}

		void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
		{
			// Only handle orientation changes when dialog is actually showing
			if (_dialog is not null && _dialog.IsShowing)
			{
				// Unsubscribe first to prevent the DismissEvent from hiding the new dialog
				_dialog.DismissEvent -= OnDialogDismiss;
				_dialog.Dismiss();
				_dialog = null;

				// Recreate dialog with current values to handle orientation change
				ShowPickerDialog(VirtualView?.Time);
			}
		}
	}
}