using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : AbstractViewHandler<IDatePicker, MauiDatePicker>
	{
		static AlertDialog? Dialog;

		protected override MauiDatePicker CreateNativeView()
		{
			return new MauiDatePicker(Context)
			{
				ShowPicker = ShowPickerDialog,
				HidePicker = HidePickerDialog
			};
		}

		protected override void DisconnectHandler(MauiDatePicker nativeView)
		{
			if (Dialog != null)
			{
				Dialog.Hide();
				Dialog.Dispose();
				Dialog = null;
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
			handler.TypedNativeView?.UpdateMinimumDate(datePicker, Dialog as DatePickerDialog);
		}

		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateMaximumDate(datePicker, Dialog as DatePickerDialog);
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
			Dialog = CreateDatePickerDialog(year, month, day);
			Dialog.Show();
		}

		void HidePickerDialog()
		{
			Dialog?.Hide();
		}
	}
}