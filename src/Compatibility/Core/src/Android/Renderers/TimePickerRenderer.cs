using System;
using System.ComponentModel;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Text.Format;
using Android.Util;
using Android.Widget;
using Microsoft.Maui.Controls.Platform;
using ATimePicker = Android.Widget.TimePicker;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public abstract class TimePickerRendererBase<TControl> : ViewRenderer<TimePicker, TControl>, TimePickerDialog.IOnTimeSetListener, IPickerRenderer
		where TControl : global::Android.Views.View
	{
		AlertDialog _dialog;
		bool _disposed;

		[PortHandler]
		bool Is24HourView
		{
			get => (DateFormat.Is24HourFormat(Context) && Element.Format == (string)TimePicker.FormatProperty.DefaultValue) || Element.Format?.Contains('H', StringComparison.Ordinal) == true;
		}

		public TimePickerRendererBase(Context context) : base(context)
		{
			AutoPackage = false;
		}

		protected abstract EditText EditText { get; }

		IElementController ElementController => Element as IElementController;

		void TimePickerDialog.IOnTimeSetListener.OnTimeSet(ATimePicker view, int hourOfDay, int minute)
		{
			ElementController.SetValueFromRenderer(TimePicker.TimeProperty, new TimeSpan(hourOfDay, minute, 0));
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);

			_dialog.CancelEvent -= OnCancelButtonClicked;
			_dialog = null;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var textField = CreateNativeControl();

				SetNativeControl(textField);
			}

			SetTime(e.NewElement.Time);
			UpdateTextColor();
			UpdateCharacterSpacing();
			UpdateFont();

			Control.TextAlignment = global::Android.Views.TextAlignment.ViewStart;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName || e.PropertyName == TimePicker.FormatProperty.PropertyName)
				SetTime(Element.Time);
			else if (e.PropertyName == TimePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == TimePicker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == TimePicker.FontAttributesProperty.PropertyName || e.PropertyName == TimePicker.FontFamilyProperty.PropertyName || e.PropertyName == TimePicker.FontSizeProperty.PropertyName)
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
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);

				_dialog.CancelEvent -= OnCancelButtonClicked;
				_dialog?.Dispose();
				_dialog = null;
			}
		}

		protected virtual TimePickerDialog CreateTimePickerDialog(int hours, int minutes)
		{
			var dialog = new TimePickerDialog(Context, this, hours, minutes, Is24HourView);
			dialog.CancelEvent += OnCancelButtonClicked;
			return dialog;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (_dialog.IsAlive())
					_dialog.CancelEvent -= OnCancelButtonClicked;

				_dialog?.Dispose();
				_dialog = null;
			}

			base.Dispose(disposing);
		}

		void IPickerRenderer.OnClick()
		{
			if (_dialog != null && _dialog.IsShowing)
			{
				return;
			}

			TimePicker view = Element;
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);

			_dialog = CreateTimePickerDialog(view.Time?.Hours ?? 0, view.Time?.Minutes ?? 0);
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

		[PortHandler]
		void SetTime(TimeSpan? time)
		{
			if (String.IsNullOrEmpty(Element.Format))
			{
				var timeFormat = "t";
				EditText.Text = DateTime.Today.Add(time ?? TimeSpan.Zero).ToString(timeFormat);
			}
			else
			{
				var timeFormat = Element.Format;
				EditText.Text = DateTime.Today.Add(time ?? TimeSpan.Zero).ToString(timeFormat);
			}

			Element.InvalidateMeasureNonVirtual(Internals.InvalidationTrigger.MeasureChanged);
		}

		[PortHandler]
		void UpdateFont()
		{
			EditText.Typeface = Element.ToTypeface();
			EditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		[PortHandler]
		void UpdateCharacterSpacing()
		{
			EditText.LetterSpacing = Element.CharacterSpacing.ToEm();
		}

		[PortHandler]
		abstract protected void UpdateTextColor();
	}

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class TimePickerRenderer : TimePickerRendererBase<EditText>
	{
		TextColorSwitcher _textColorSwitcher;

		public TimePickerRenderer(Context context) : base(context)
		{
		}

		protected override EditText CreateNativeControl()
		{
			return new PickerEditText(Context);
		}

		protected override EditText EditText => Control;

		[PortHandler]
		protected override void UpdateTextColor()
		{
			_textColorSwitcher = _textColorSwitcher ?? new TextColorSwitcher(EditText.TextColors, Element.UseLegacyColorManagement());
			_textColorSwitcher.UpdateTextColor(EditText, Element.TextColor);
		}
	}
}
