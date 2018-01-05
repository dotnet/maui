using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Widget;
using Android.Text.Format;
using ADatePicker = Android.Widget.DatePicker;
using ATimePicker = Android.Widget.TimePicker;
using Object = Java.Lang.Object;
using Android.OS;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, EditText>, TimePickerDialog.IOnTimeSetListener
	{
		AlertDialog _dialog;
		TextColorSwitcher _textColorSwitcher;
		bool _is24HourFormat;
		string _timeFormat;

		public TimePickerRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use TimePickerRenderer(Context) instead.")]
		public TimePickerRenderer()
		{
			AutoPackage = false;
		}

		IElementController ElementController => Element as IElementController;

		void TimePickerDialog.IOnTimeSetListener.OnTimeSet(ATimePicker view, int hourOfDay, int minute)
		{
			ElementController.SetValueFromRenderer(TimePicker.TimeProperty, new TimeSpan(hourOfDay, minute, 0));

			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
			Control.ClearFocus();

			if (Forms.IsLollipopOrNewer)
				_dialog.CancelEvent -= OnCancelButtonClicked;

			_dialog = null;
		}

		protected override EditText CreateNativeControl()
		{
			return new EditText(Context) { Focusable = false, Clickable = true, Tag = this };
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var textField = CreateNativeControl();

				textField.SetOnClickListener(TimePickerListener.Instance);
				SetNativeControl(textField);

				var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
				_textColorSwitcher = new TextColorSwitcher(textField.TextColors, useLegacyColorManagement);
				
				_is24HourFormat = DateFormat.Is24HourFormat(Context);
				_timeFormat = _is24HourFormat ? "HH:mm" : Element.Format;
			}

			SetTime(e.NewElement.Time);
			UpdateTextColor();

			if ((int)Build.VERSION.SdkInt > 16)
				Control.TextAlignment = global::Android.Views.TextAlignment.ViewStart;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName ||
				e.PropertyName == TimePicker.FormatProperty.PropertyName)
				SetTime(Element.Time);
			else if (e.PropertyName == TimePicker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == TimePicker.FontAttributesProperty.PropertyName || e.PropertyName == TimePicker.FontFamilyProperty.PropertyName || e.PropertyName == TimePicker.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		internal override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
				OnClick();
			else if (_dialog != null)
			{
				_dialog.Hide();
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				Control.ClearFocus();

				if (Forms.IsLollipopOrNewer)
					_dialog.CancelEvent -= OnCancelButtonClicked;

				_dialog = null;
			}
		}

		void OnClick()
		{
			TimePicker view = Element;
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);

			_dialog = new TimePickerDialog(Context, this, view.Time.Hours, view.Time.Minutes, _is24HourFormat);

			if (Forms.IsLollipopOrNewer)
				_dialog.CancelEvent += OnCancelButtonClicked;

			_dialog.Show();
		}

		void OnCancelButtonClicked(object sender, EventArgs e)
		{
			Element.Unfocus();
		}

		void SetTime(TimeSpan time)
		{
			Control.Text = DateTime.Today.Add(time).ToString(_timeFormat);
		}

		void UpdateFont()
		{
			Control.Typeface = Element.ToTypeface();
			Control.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(Control, Element.TextColor);
		}

		class TimePickerListener : Object, IOnClickListener
		{
			public static readonly TimePickerListener Instance = new TimePickerListener();

			public void OnClick(global::Android.Views.View v)
			{
				var renderer = v.Tag as TimePickerRenderer;
				if (renderer == null)
					return;

				renderer.OnClick();
			}
		}
	}
}
