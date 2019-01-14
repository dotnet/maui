using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, EditText>, IPickerRenderer
	{
		DatePickerDialog _dialog;
		bool _disposed;
		TextColorSwitcher _textColorSwitcher;

		public DatePickerRenderer(Context context) : base(context)
		{
			AutoPackage = false;
			if (Forms.IsLollipopOrNewer)
				Device.Info.PropertyChanged += DeviceInfoPropertyChanged;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use DatePickerRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DatePickerRenderer()
		{
			AutoPackage = false;
			if (Forms.IsLollipopOrNewer)
				Device.Info.PropertyChanged += DeviceInfoPropertyChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (Forms.IsLollipopOrNewer)
					Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;

				_disposed = true;
				if (_dialog != null)
				{
					if (Forms.IsLollipopOrNewer)
						_dialog.CancelEvent -= OnCancelButtonClicked;

					_dialog.Hide();
					_dialog.Dispose();
					_dialog = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override EditText CreateNativeControl()
		{
			return new PickerEditText(Context, this);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var textField = CreateNativeControl();

				SetNativeControl(textField);

				var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
				_textColorSwitcher = new TextColorSwitcher(textField.TextColors, useLegacyColorManagement); 
			}

			SetDate(Element.Date);

			UpdateFont();
			UpdateMinimumDate();
			UpdateMaximumDate();
			UpdateTextColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName || e.PropertyName == DatePicker.FormatProperty.PropertyName)
				SetDate(Element.Date);
			else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
				UpdateMinimumDate();
			else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
				UpdateMaximumDate();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == DatePicker.FontAttributesProperty.PropertyName || e.PropertyName == DatePicker.FontFamilyProperty.PropertyName || e.PropertyName == DatePicker.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
				CallOnClick();
			else if (_dialog != null)
			{
				_dialog.Hide();
				((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);

				if (Forms.IsLollipopOrNewer)
					_dialog.CancelEvent -= OnCancelButtonClicked;

				_dialog = null;
			}
		}

		protected virtual DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
		{
			DatePicker view = Element;
			var dialog = new DatePickerDialog(Context, (o, e) =>
			{
				view.Date = e.Date;
				((IElementController)view).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
			}, year, month, day);

			return dialog;
		}

		void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentOrientation")
			{
				DatePickerDialog currentDialog = _dialog;
				if (currentDialog != null && currentDialog.IsShowing)
				{
					currentDialog.Dismiss();
					if (Forms.IsLollipopOrNewer)
						currentDialog.CancelEvent -= OnCancelButtonClicked;

					ShowPickerDialog(currentDialog.DatePicker.Year, currentDialog.DatePicker.Month, currentDialog.DatePicker.DayOfMonth);
				}
			}
		}

		void IPickerRenderer.OnClick()
		{
			if (_dialog != null && _dialog.IsShowing)
			{
				return;
			}

			DatePicker view = Element;
			((IElementController)view).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);

			ShowPickerDialog(view.Date.Year, view.Date.Month - 1, view.Date.Day);
		}

		void ShowPickerDialog(int year, int month, int day)
		{
			_dialog = CreateDatePickerDialog(year, month, day);

			UpdateMinimumDate();
			UpdateMaximumDate();

			if (Forms.IsLollipopOrNewer)
				_dialog.CancelEvent += OnCancelButtonClicked;

			_dialog.Show();
		}

		void OnCancelButtonClicked(object sender, EventArgs e)
		{
			Element.Unfocus();
		}

		void SetDate(DateTime date)
		{
			Control.Text = date.ToString(Element.Format);
		}

		void UpdateFont()
		{
			Control.Typeface = Element.ToTypeface();
			Control.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateMaximumDate()
		{
			if (_dialog != null)
			{
				_dialog.DatePicker.MaxDate = (long)Element.MaximumDate.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		void UpdateMinimumDate()
		{
			if (_dialog != null)
			{
				_dialog.DatePicker.MinDate = (long)Element.MinimumDate.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(Control, Element.TextColor);
		}
	}
}