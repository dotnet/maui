using System;
using System.ComponentModel;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public abstract class DatePickerRendererBase<TControl> : ViewRenderer<DatePicker, TControl>, IPickerRenderer
		where TControl : global::Android.Views.View
	{
		int _originalHintTextColor;
		DatePickerDialog _dialog;
		bool _disposed;
		protected abstract EditText EditText { get; }

		public DatePickerRendererBase(Context context) : base(context)
		{
			AutoPackage = false;
			DeviceDisplay.MainDisplayInfoChanged += DeviceInfoPropertyChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				DeviceDisplay.MainDisplayInfoChanged -= DeviceInfoPropertyChanged;

				_disposed = true;
				if (_dialog != null)
				{
					_dialog.CancelEvent -= OnCancelButtonClicked;
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
				var textField = CreateNativeControl();
				SetNativeControl(textField);
				_originalHintTextColor = EditText.CurrentHintTextColor;
			}

			SetDate(Element.Date);

			UpdateFont();
			UpdateMinimumDate();
			UpdateMaximumDate();
			UpdateTextColor();
			UpdateCharacterSpacing();
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
			else if (e.PropertyName == DatePicker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == DatePicker.FontAttributesProperty.PropertyName || e.PropertyName == DatePicker.FontFamilyProperty.PropertyName || e.PropertyName == DatePicker.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
			{
				if (Clickable)
					CallOnClick();
				else
					((IPickerRenderer)this)?.OnClick();
			}
			else if (_dialog != null)
			{
				_dialog.Hide();
				((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
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

		[PortHandler]
		void DeviceInfoPropertyChanged(object sender, DisplayInfoChangedEventArgs e)
		{
			DatePickerDialog currentDialog = _dialog;
			if (currentDialog != null && currentDialog.IsShowing)
			{
				currentDialog.Dismiss();
				currentDialog.CancelEvent -= OnCancelButtonClicked;

				ShowPickerDialog(currentDialog.DatePicker.Year, currentDialog.DatePicker.Month, currentDialog.DatePicker.DayOfMonth);
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

			var year = view.Date?.Year ?? DateTime.Today.Year;
			var month = (view.Date?.Month ?? DateTime.Today.Month) - 1;
			var day = view.Date?.Day ?? DateTime.Today.Day;

			ShowPickerDialog(year, month, day);
		}

		void ShowPickerDialog(int year, int month, int day)
		{
			_dialog = CreateDatePickerDialog(year, month, day);

			UpdateMinimumDate();
			UpdateMaximumDate();
			_dialog.CancelEvent += OnCancelButtonClicked;

			_dialog.Show();
		}

		[PortHandler]
		void OnCancelButtonClicked(object sender, EventArgs e)
		{
			// I would say the original bugzilla issue that added this code is wrong
			// https://bugzilla.xamarin.com/42/42074/bug.html
			// I don't see why cancelling the popup would cause the focus to remove from the control
			// That's the control the user clicked on
			// I'm pretty sure this was initially done to match the iOS behavior but we shouldn't just
			// match focus behavior for no good reason.
			// On WinUI when the calendar control opens the TextBox loses focus then gains it back when you close
			// Which is also how Android works
			Element.Unfocus();
		}

		void SetDate(DateTime? date)
		{
			if (String.IsNullOrWhiteSpace(Element.Format))
			{
				EditText.Text = date?.ToShortDateString();
			}
			else if (Element.Format.Contains('/', StringComparison.Ordinal))
			{
				EditText.Text = date?.ToString(Element.Format, CultureInfo.InvariantCulture);
			}
			else
			{
				EditText.Text = date?.ToString(Element.Format);
			}
		}

		[PortHandler]
		void UpdateCharacterSpacing()
		{
			EditText.LetterSpacing = Element.CharacterSpacing.ToEm();
		}

		[PortHandler]
		void UpdateFont()
		{
			EditText.Typeface = Element.ToTypeface();
			EditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		[PortHandler]
		void UpdateMaximumDate()
		{
			if (_dialog != null)
			{
				if (Element.MaximumDate is null)
				{
					_dialog.DatePicker.MaxDate = (long)DateTime.MaxValue.ToUniversalTime()
						.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;

					return;
				}

				_dialog.DatePicker.MaxDate = (long)Element.MaximumDate.Value
					.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		[PortHandler]
		void UpdateMinimumDate()
		{
			if (_dialog != null)
			{
				if (Element.MinimumDate is null)
				{
					_dialog.DatePicker.MinDate = (long)DateTime.MinValue.ToUniversalTime()
						.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;

					return;
				}

				_dialog.DatePicker.MinDate = (long)Element.MinimumDate.Value
					.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		abstract protected void UpdateTextColor();
	}


	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class DatePickerRenderer : DatePickerRendererBase<EditText>
	{
		TextColorSwitcher _textColorSwitcher;

		public DatePickerRenderer(Context context) : base(context)
		{
		}

		protected override EditText CreateNativeControl()
		{
			return new PickerEditText(Context);
		}

		protected override EditText EditText => Control;

		protected override void UpdateTextColor()
		{
			_textColorSwitcher = _textColorSwitcher ?? new TextColorSwitcher(EditText.TextColors, Element.UseLegacyColorManagement());
			_textColorSwitcher.UpdateTextColor(EditText, Element.TextColor);
		}
	}
}