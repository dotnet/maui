using Android.App;
using AndroidX.AppCompat.Widget;

namespace System.Maui.Platform
{
	public partial class DatePickerRenderer : AbstractViewRenderer<IDatePicker, AppCompatTextView>
	{
		TextColorSwitcher _textColorSwitcher;
		DatePickerDialog _dialog;

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

		public static void MapPropertyMaximumDate(IViewRenderer renderer, IDatePicker datePicker)
		{
			(renderer as DatePickerRenderer)?.UpdateMaximumDate();
		}

		public static void MapPropertyMinimumDate(IViewRenderer renderer, IDatePicker datePicker)
		{
			(renderer as DatePickerRenderer)?.UpdateMinimumDate();
		}

		protected virtual DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
		{
			void onDateSetCallback(object obj, DatePickerDialog.DateSetEventArgs args)
			{
				VirtualView.SelectedDate = args.Date;
				TypedNativeView.Text = VirtualView.Text;
			}

			var dialog = new DatePickerDialog(Context, onDateSetCallback, year, month, day);

			return dialog;
		}

		private void ShowPickerDialog() 
		{
			var date = VirtualView.SelectedDate;
			ShowPickerDialog(date.Year, date.Month, date.Day);
		}

		// This overload is here so we can pass in the current values from the dialog 
		// on an orientation change (so that orientation changes don't cause the user's date selection progress
		// to be lost). Not useful until we have orientation changed events.
		private void ShowPickerDialog(int year, int month, int day)
		{
			_dialog = CreateDatePickerDialog(year, month, day);

			UpdateMinimumDate();
			UpdateMaximumDate();

			_dialog.Show();
		}

		private void HidePickerDialog() 
		{
			_dialog?.Hide();
		}

		private long ConvertDate(DateTime date) 
		{
			return (long)date.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
		}

		private void UpdateMaximumDate()
		{
			if (_dialog == null)
			{
				return;
			}

			_dialog.DatePicker.MaxDate = ConvertDate(VirtualView.MaximumDate);
		}

		private void UpdateMinimumDate()
		{
			if (_dialog == null)
			{
				return;
			}

			_dialog.DatePicker.MinDate = ConvertDate(VirtualView.MinimumDate);
		}
	}
}
