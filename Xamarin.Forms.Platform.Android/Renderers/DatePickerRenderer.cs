using System;
using System.ComponentModel;
using Android.App;
using Android.Widget;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	public class DatePickerRenderer : ViewRenderer<DatePicker, EditText>
	{
		DatePickerDialog _dialog;
		bool _disposed;

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
					_dialog.Hide();
					_dialog.Dispose();
					_dialog = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var textField = new EditText(Context) { Focusable = false, Clickable = true, Tag = this };

				textField.SetOnClickListener(TextFieldClickHandler.Instance);
				SetNativeControl(textField);
			}

			SetDate(Element.Date);

			UpdateMinimumDate();
			UpdateMaximumDate();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "Date" || e.PropertyName == DatePicker.FormatProperty.PropertyName)
				SetDate(Element.Date);
			else if (e.PropertyName == "MinimumDate")
				UpdateMinimumDate();
			else if (e.PropertyName == "MaximumDate")
				UpdateMaximumDate();
		}

		internal override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
				OnTextFieldClicked();
			else if (_dialog != null)
			{
				_dialog.Hide();
				((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				Control.ClearFocus();
				_dialog = null;
			}
		}

		void CreateDatePickerDialog(int year, int month, int day)
		{
			DatePicker view = Element;
			_dialog = new DatePickerDialog(Context, (o, e) =>
			{
				view.Date = e.Date;
				((IElementController)view).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				Control.ClearFocus();
				_dialog = null;
			}, year, month, day);
		}

		void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentOrientation")
			{
				DatePickerDialog currentDialog = _dialog;
				if (currentDialog != null && currentDialog.IsShowing)
				{
					currentDialog.Dismiss();
					CreateDatePickerDialog(currentDialog.DatePicker.Year, currentDialog.DatePicker.Month, currentDialog.DatePicker.DayOfMonth);
					_dialog.Show();
				}
			}
		}

		void OnTextFieldClicked()
		{
			DatePicker view = Element;
			((IElementController)view).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);

			CreateDatePickerDialog(view.Date.Year, view.Date.Month - 1, view.Date.Day);

			UpdateMinimumDate();
			UpdateMaximumDate();
			_dialog.Show();
		}

		void SetDate(DateTime date)
		{
			Control.Text = date.ToString(Element.Format);
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

		class TextFieldClickHandler : Object, IOnClickListener
		{
			public static readonly TextFieldClickHandler Instance = new TextFieldClickHandler();

			public void OnClick(AView v)
			{
				((DatePickerRenderer)v.Tag).OnTextFieldClicked();
			}
		}
	}
}