using System;
using Android.App;
using Android.Graphics.Drawables;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		static Drawable? DefaultBackground;

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

		protected override void SetupDefaults(MauiDatePicker nativeView)
		{
			DefaultBackground = nativeView.Background;

			base.SetupDefaults(nativeView);
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

		// This is a Android-specific mapping
		public static void MapBackground(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateBackground(datePicker, DefaultBackground);
		}

		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateFormat(datePicker);
		}

		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateDate(datePicker);
		}

		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateMinimumDate(datePicker, handler._dialog);
		}

		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateMaximumDate(datePicker, handler._dialog);
		}

		public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateCharacterSpacing(datePicker);
		}

		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(datePicker, fontManager);
		}

		[MissingMapper]
		public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker) { }

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