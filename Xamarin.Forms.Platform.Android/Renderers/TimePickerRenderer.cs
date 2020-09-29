using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text.Format;
using Android.Util;
using Android.Widget;
using AColor = Android.Graphics.Color;
using ATimePicker = Android.Widget.TimePicker;

namespace Xamarin.Forms.Platform.Android
{
	public abstract class TimePickerRendererBase<TControl> : ViewRenderer<TimePicker, TControl>, TimePickerDialog.IOnTimeSetListener, IPickerRenderer
		where TControl : global::Android.Views.View
	{
		AlertDialog _dialog;
		bool _disposed;

		bool Is24HourView
		{
			get => (DateFormat.Is24HourFormat(Context) && Element.Format == (string)TimePicker.FormatProperty.DefaultValue) || Element.Format == "HH:mm";
		}

		public TimePickerRendererBase(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use TimePickerRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TimePickerRendererBase()
		{
			AutoPackage = false;
		}

		protected abstract EditText EditText { get; }

		IElementController ElementController => Element as IElementController;

		void TimePickerDialog.IOnTimeSetListener.OnTimeSet(ATimePicker view, int hourOfDay, int minute)
		{
			ElementController.SetValueFromRenderer(TimePicker.TimeProperty, new TimeSpan(hourOfDay, minute, 0));
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);

			if (Forms.IsLollipopOrNewer)
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

			if ((int)Forms.SdkInt > 16)
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

				if (Forms.IsLollipopOrNewer)
					_dialog.CancelEvent -= OnCancelButtonClicked;

				_dialog?.Dispose();
				_dialog = null;
			}
		}

		protected virtual TimePickerDialog CreateTimePickerDialog(int hours, int minutes)
		{
			var dialog = new TimePickerDialog(Context, this, hours, minutes, Is24HourView);

			if (Forms.IsLollipopOrNewer)
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
				if (Forms.IsLollipopOrNewer && _dialog.IsAlive())
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

			_dialog = CreateTimePickerDialog(view.Time.Hours, view.Time.Minutes);
			_dialog.Show();
		}

		void OnCancelButtonClicked(object sender, EventArgs e)
		{
			Element.Unfocus();
		}

		void SetTime(TimeSpan time)
		{
			var timeFormat = Is24HourView ? "HH:mm" : Element.Format;
			EditText.Text = DateTime.Today.Add(time).ToString(timeFormat);
			Element.InvalidateMeasureNonVirtual(Internals.InvalidationTrigger.MeasureChanged);
		}

		void UpdateFont()
		{
			EditText.Typeface = Element.ToTypeface();
			EditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer)
			{
				EditText.LetterSpacing = Element.CharacterSpacing.ToEm();
			}
		}

		abstract protected void UpdateTextColor();
	}

	public class TimePickerRenderer : TimePickerRendererBase<EditText>
	{
		TextColorSwitcher _textColorSwitcher;
		[Obsolete("This constructor is obsolete as of version 2.5. Please use TimePickerRenderer(Context) instead.")]
		public TimePickerRenderer()
		{
		}

		public TimePickerRenderer(Context context) : base(context)
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