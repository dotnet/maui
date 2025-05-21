using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text.Format;
using DateFormat = Android.Text.Format.DateFormat;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		TimePickerDialogDismissListener DialogDismissListener { get; } = new TimePickerDialogDismissListener();
		
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
				DialogDismissListener.Handler = null;
				_dialog.SetOnDismissListener(null);
				
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
				VirtualView.IsFocused = false;

				if (_dialog != null)
				{
					_dialog = null;
				}
			}

			var dialog = new TimePickerDialog(Context!, onTimeSetCallback, hour, minute, Use24HourView);
			
			DialogDismissListener.Handler = this;
			dialog.SetOnDismissListener(DialogDismissListener);
			
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

		public static void MapIsOpen(ITimePickerHandler handler, ITimePicker timePicker)
		{
			if (handler is TimePickerHandler timePickerHandler)
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
				_dialog.Hide();
			}

			_dialog = null;

			if (VirtualView is not null)
			{
				VirtualView.IsOpen = false;
			}
		}

		bool Use24HourView => VirtualView != null && (DateFormat.Is24HourFormat(PlatformView?.Context)
			&& VirtualView.Format == "t" || VirtualView.Format == "HH:mm");
		
		static void OnDismiss(ITimePickerHandler? handler, IDialogInterface? dialog)
		{
			if (handler is TimePickerHandler timePickerHandler)
				timePickerHandler.HidePickerDialog();
		}
		
		class TimePickerDialogDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
		{
			public TimePickerHandler? Handler { get; set; }
			
			public void OnDismiss(IDialogInterface? dialog)
			{
				TimePickerHandler.OnDismiss(Handler, dialog);
			}
		}
	}
}