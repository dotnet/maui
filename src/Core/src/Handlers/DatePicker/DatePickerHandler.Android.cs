using Android.App;
using Android.Content.Res;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		Drawable? _defaultBackground;
		ColorStateList? _defaultTextColors;

		DatePickerDialog? _dialog;

		protected override MauiDatePicker CreatePlatformView()
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

		protected override void ConnectHandler(MauiDatePicker nativeView)
		{
			base.ConnectHandler(nativeView);

			SetupDefaults(nativeView);
		}

		void SetupDefaults(MauiDatePicker nativeView)
		{
			_defaultBackground = nativeView.Background;
			_defaultTextColors = nativeView.TextColors;
		}

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

				// TODO: Update IsFocused Property

			}, year, month, day);

			return dialog;
		}

		// This is a Android-specific mapping
		public static void MapBackground(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateBackground(datePicker, handler._defaultBackground);
		}

		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateDate(datePicker);
		}

		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMinimumDate(datePicker, handler._dialog);
		}

		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMaximumDate(datePicker, handler._dialog);
		}

		public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(datePicker);
		}

		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateTextColor(datePicker, handler._defaultTextColors);
		}

		void ShowPickerDialog()
		{
			if (VirtualView == null)
				return;

			if (_dialog != null && _dialog.IsShowing)
				return;

			var date = VirtualView.Date;
			ShowPickerDialog(date.Year, date.Month - 1, date.Day);
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