using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class TimePickerRenderer : TimePickerRendererBase<UITextField>
	{
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public TimePickerRenderer()
		{

		}

		[PortHandler]
		protected override UITextField CreateNativeControl()
		{
			return new NoCaretField { BorderStyle = UITextBorderStyle.RoundedRect };
		}
	}

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public abstract class TimePickerRendererBase<TControl> : ViewRenderer<TimePicker, TControl>
		where TControl : UITextField
	{
		UIDatePicker _picker;
		UIColor _defaultTextColor;
		bool _disposed;
		bool _useLegacyColorManagement;

		internal UIDatePicker Picker => _picker;

		IElementController ElementController => Element as IElementController;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public TimePickerRendererBase()
		{

		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_defaultTextColor = null;

				if (_picker != null)
				{
					_picker.RemoveFromSuperview();
					_picker.ValueChanged -= OnValueChanged;
					_picker.Dispose();
					_picker = null;
				}

				if (Control != null)
				{
					Control.EditingDidBegin -= OnStarted;
					Control.EditingDidEnd -= OnEnded;
				}
			}

			base.Dispose(disposing);
		}


		protected abstract override TControl CreateNativeControl();

		[PortHandler]
		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var entry = CreateNativeControl();

					entry.EditingDidBegin += OnStarted;
					entry.EditingDidEnd += OnEnded;

					_picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };

					if (Forms.IsiOS14OrNewer)
					{
						_picker.PreferredDatePickerStyle = UIKit.UIDatePickerStyle.Wheels;
					}

					var width = UIScreen.MainScreen.Bounds.Width;
					var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
					var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
					var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
					{
						UpdateElementTime();
						entry.ResignFirstResponder();
					});

					toolbar.SetItems(new[] { spacer, doneButton }, false);

					entry.InputView = _picker;
					entry.InputAccessoryView = toolbar;

					entry.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
					entry.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

					entry.InputAssistantItem.LeadingBarButtonGroups = null;
					entry.InputAssistantItem.TrailingBarButtonGroups = null;

					_defaultTextColor = entry.TextColor;

					_useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();

					_picker.ValueChanged += OnValueChanged;

					entry.AccessibilityTraits = UIAccessibilityTrait.Button;

					SetNativeControl(entry);
				}

				UpdateFont();
				UpdateTime();
				UpdateTextColor();
				UpdateCharacterSpacing();
				UpdateFlowDirection();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName || e.PropertyName == TimePicker.FormatProperty.PropertyName)
			{
				UpdateTime();
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == TimePicker.TextColorProperty.PropertyName || e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == TimePicker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == TimePicker.FontAttributesProperty.PropertyName ||
					 e.PropertyName == TimePicker.FontFamilyProperty.PropertyName || e.PropertyName == TimePicker.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		[PortHandler]
		void OnEnded(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		[PortHandler]
		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		[PortHandler]
		void OnValueChanged(object sender, EventArgs e)
		{
			if (Element.OnThisPlatform().UpdateMode() == UpdateMode.Immediately)
			{
				UpdateElementTime();
			}
		}

		void UpdateFlowDirection()
		{
			(Control as UITextField).UpdateTextAlignment(Element);
		}

		[PortHandler]
		protected internal virtual void UpdateFont()
		{
			Control.Font = Element.ToUIFont();
		}

		[PortHandler]
		protected internal virtual void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			if (textColor == null || (!Element.IsEnabled && _useLegacyColorManagement))
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToPlatform();

			// HACK This forces the color to update; there's probably a more elegant way to make this happen
			Control.Text = Control.Text;
		}

		[PortHandler]
		void UpdateCharacterSpacing()
		{
			var textAttr = Control.AttributedText.WithCharacterSpacing(Element.CharacterSpacing);

			if (textAttr != null)
				Control.AttributedText = textAttr;
		}

		[PortHandler]
		void UpdateTime()
		{
			_picker.Date = new DateTime(1, 1, 1).Add(Element.Time ?? TimeSpan.Zero).ToNSDate();
			string iOSLocale = NSLocale.CurrentLocale.CountryCode;
			var cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
							  .Where(c => c.Name.EndsWith("-" + iOSLocale)).FirstOrDefault();
			if (cultureInfos == null)
				cultureInfos = CultureInfo.InvariantCulture;

			if (String.IsNullOrEmpty(Element.Format))
			{
				string timeformat = cultureInfos.DateTimeFormat.ShortTimePattern;
				NSLocale locale = new NSLocale(cultureInfos.TwoLetterISOLanguageName);
				Control.Text = DateTime.Today.Add(Element.Time ?? TimeSpan.Zero).ToString(timeformat, cultureInfos);
				_picker.Locale = locale;
			}
			else
			{
				Control.Text = DateTime.Today.Add(Element.Time ?? TimeSpan.Zero).ToString(Element.Format, cultureInfos);
			}

			if (Element.Format?.Contains('H', StringComparison.Ordinal) == true)
			{
				var ci = new System.Globalization.CultureInfo("de-DE");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);
				_picker.Locale = locale;
			}
			else if (Element.Format?.Contains('h', StringComparison.Ordinal) == true)
			{
				var ci = new System.Globalization.CultureInfo("en-US");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);
				_picker.Locale = locale;
			}
			Element.InvalidateMeasureNonVirtual(Controls.Internals.InvalidationTrigger.MeasureChanged);
		}

		[PortHandler]
		void UpdateElementTime()
		{
			ElementController.SetValueFromRenderer(TimePicker.TimeProperty, _picker.Date.ToDateTime() - new DateTime(1, 1, 1));
		}
	}
}