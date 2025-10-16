using System;
using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text.Format;
using Android.Views;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		MauiTimePicker? _timePicker;
		TimePickerDialog? _dialog;
		int _currentHour;
		int _currentMinute;

		protected override MauiTimePicker CreatePlatformView()
		{
			_timePicker = new MauiTimePicker(Context)
			{
				ShowPicker = ShowPickerDialog,
				HidePicker = HidePickerDialog
			};

			return _timePicker;
		}

		protected override void ConnectHandler(MauiTimePicker platformView)
		{
			base.ConnectHandler(platformView);
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
				_dialog.Hide();
				_dialog = null;
			}

			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
			OnViewDetachedFromWindow();

			base.DisconnectHandler(platformView);
		}

		protected virtual TimePickerDialog CreateTimePickerDialog(int hour, int minute)
		{
			// Store the current values for orientation change handling
			_currentHour = hour;
			_currentMinute = minute;

			void onTimeSetCallback(object? obj, TimePickerDialog.TimeSetEventArgs args)
			{
				if (VirtualView == null || PlatformView == null)
					return;

				// Update stored values when user selects time
				_currentHour = args.HourOfDay;
				_currentMinute = args.Minute;

				VirtualView.Time = new TimeSpan(args.HourOfDay, args.Minute, 0);
				VirtualView.IsFocused = false;

				if (_dialog != null)
				{
					_dialog = null;
				}
			}

			var dialog = new TimePickerDialog(Context!, onTimeSetCallback, hour, minute, Use24HourView);

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

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker);
		}

		void ShowPickerDialog()
		{
			if (VirtualView == null)
				return;

			var time = VirtualView.Time;
			ShowPickerDialog(time.Hours, time.Minutes);
		}

		// This overload is here so we can pass in the current values from the dialog 
		// on an orientation change (so that orientation changes don't cause the user's date selection progress
		// to be lost). Not useful until we have orientation changed events.
		void ShowPickerDialog(int hour, int minute)
		{
			_dialog = CreateTimePickerDialog(hour, minute);
			_dialog.Show();
		}

		void HidePickerDialog()
		{
			_dialog?.Hide();

			_dialog = null;
		}

		bool Use24HourView => VirtualView != null && (DateFormat.Is24HourFormat(PlatformView?.Context)
			&& VirtualView.Format == "t" || VirtualView.Format == "HH:mm");

		void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
		{
			// Only handle orientation changes when dialog is actually showing
			if (_dialog is not null && _dialog.IsShowing)
			{
				// Dismiss the current dialog and clear reference
				_dialog.Dismiss();
				_dialog = null;

				// Recreate dialog with current values to handle orientation change
				ShowPickerDialog(_currentHour, _currentMinute);
			}
		}
	}
}