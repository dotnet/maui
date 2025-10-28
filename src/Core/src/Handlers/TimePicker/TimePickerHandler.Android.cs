using System;
using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text.Format;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		MauiTimePicker? _timePicker;
		TimePickerDialog? _dialog;

		protected override MauiTimePicker CreatePlatformView()
		{
			_timePicker = new MauiTimePicker(Context)
			{
				ShowPicker = ShowPickerDialog,
				HidePicker = HidePickerDialog
			};

			return _timePicker;
		}

		protected override void DisconnectHandler(MauiTimePicker platformView)
		{
			if (_dialog != null)
			{
				_dialog.Dismiss();
				_dialog = null;
			}
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

		// Make it public in .NET 10.
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
	}
}