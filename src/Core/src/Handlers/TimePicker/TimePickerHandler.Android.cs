using System;
using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text.Format;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		static Drawable? DefaultBackground;
		static ColorStateList? DefaultTextColors;

		MauiTimePicker? _timePicker;
		AlertDialog? _dialog;

		protected override MauiTimePicker CreatePlatformView()
		{
			_timePicker = new MauiTimePicker(Context)
			{
				ShowPicker = ShowPickerDialog,
				HidePicker = HidePickerDialog
			};

			return _timePicker;
		}

		protected override void DisconnectHandler(MauiTimePicker nativeView)
		{
			if (_dialog != null)
			{
				_dialog.Hide();
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
			}

			var dialog = new TimePickerDialog(Context!, onTimeSetCallback, hour, minute, Use24HourView);

			return dialog;
		}

		// This is a Android-specific mapping
		public static void MapBackground(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateBackground(timePicker, DefaultBackground);
		}

		public static void MapFormat(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFormat(timePicker);
		}

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker);
		}

		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(TimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker, DefaultTextColors);
		}

		static void SetupDefaults(MauiTimePicker nativeView)
		{
			DefaultBackground = nativeView.Background;
			DefaultTextColors = nativeView.TextColors;
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