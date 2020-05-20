using Android.App;
using AndroidX.AppCompat.Widget;

namespace System.Maui.Platform
{
	public partial class TimePickerRenderer : AbstractViewRenderer<ITimePicker, AppCompatTextView>
	{
		TextColorSwitcher _textColorSwitcher;
		TimePickerDialog _dialog;

		protected override AppCompatTextView CreateView()
		{
			var text = new PickerView(Context)
			{
				HidePicker = HidePickerDialog,
				ShowPicker = ShowPickerDialog
			};

			_textColorSwitcher = new TextColorSwitcher(text);
			return text;
		}

		protected override void DisposeView(AppCompatTextView nativeView)
		{
			_textColorSwitcher = null;
			if (_dialog != null)
			{
				_dialog.Hide();
				_dialog = null;
			}

			base.DisposeView(nativeView);
		}

		protected virtual TimePickerDialog CreateTimePickerDialog(int hour, int minute)
		{
			void onTimeSetCallback(object obj, TimePickerDialog.TimeSetEventArgs args)
			{
				VirtualView.SelectedTime = new TimeSpan(args.HourOfDay, args.Minute, 0);
				TypedNativeView.Text = VirtualView.Text;
			}

			var dialog = new TimePickerDialog(Context, onTimeSetCallback, hour, minute, Use24HourClock());

			return dialog;
		}

		private bool Use24HourClock() 
		{
			var clock = VirtualView.ClockIdentifier;
			if (clock == string.Empty)
			{
				// No clock specified, so let's do our best to figure out the default for the locale
				if (Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("H"))
				{
					clock = ClockIdentifiers.TwentyFourHour;
				}
			}

			return clock == ClockIdentifiers.TwentyFourHour;
		}

		private void ShowPickerDialog()
		{
			var time = VirtualView.SelectedTime;
			ShowPickerDialog(time.Hours, time.Minutes);
		}

		// This overload is here so we can pass in the current values from the dialog 
		// on an orientation change (so that orientation changes don't cause the user's date selection progress
		// to be lost). Not useful until we have orientation changed events.
		private void ShowPickerDialog(int hour, int minute)
		{
			_dialog = CreateTimePickerDialog(hour, minute);
			_dialog.Show();
		}

		private void HidePickerDialog()
		{
			_dialog?.Hide();
		}

			}
}
