using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : AbstractViewHandler<IDatePicker, MauiDatePicker>
	{
		DatePickerDialog? _dialog;

		protected override MauiDatePicker CreateNativeView()
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

		protected override void DisconnectHandler(MauiDatePicker nativeView)
		{
			if (_dialog != null)
			{
				_dialog.Hide();
				_dialog.Dispose();
				_dialog = null;
			}

			base.DisconnectHandler(nativeView);
		}

		protected virtual DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
		{
			var dialog = new DatePickerDialog(Context!, (o, e) =>
			{
				if (VirtualView != null)
					VirtualView.Date = e.Date;
			}, year, month, day);

			return dialog;
		}

		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateFormat(datePicker);
		}

		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateDate(datePicker);
		}

		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateMinimumDate(datePicker, handler._dialog);
		}

		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateMaximumDate(datePicker, handler._dialog);
		}

		void ShowPickerDialog()
		{
			if (VirtualView == null)
				return;

			var date = VirtualView.Date;
			ShowPickerDialog(date.Year, date.Month, date.Day);
		}

		void ShowPickerDialog(int year, int month, int day)
		{
			if (_dialog == null)
				_dialog = CreateDatePickerDialog(year, month, day);
			else
				_dialog.UpdateDate(year, month, day);

			_dialog.Show();
		}

		void HidePickerDialog()
		{
			_dialog?.Hide();
		}
	}
}