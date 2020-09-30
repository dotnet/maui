using System;
using System.ComponentModel;
using Foundation;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	internal class NoCaretField : UITextField
	{
		public NoCaretField() : base(new RectangleF())
		{
			SpellCheckingType = UITextSpellCheckingType.No;
			AutocorrectionType = UITextAutocorrectionType.No;
			AutocapitalizationType = UITextAutocapitalizationType.None;
		}

		public override RectangleF GetCaretRectForPosition(UITextPosition position)
		{
			return new RectangleF();
		}
	}

	public class DatePickerRenderer : DatePickerRendererBase<UITextField>
	{
		[Internals.Preserve(Conditional = true)]
		public DatePickerRenderer()
		{

		}

		protected override UITextField CreateNativeControl()
		{
			return new NoCaretField { BorderStyle = UITextBorderStyle.RoundedRect };
		}
	}

	public abstract class DatePickerRendererBase<TControl> : ViewRenderer<DatePicker, TControl>
		where TControl : UITextField
	{
		UIDatePicker _picker;
		UIColor _defaultTextColor;
		bool _disposed;
		bool _useLegacyColorManagement;

		IElementController ElementController => Element as IElementController;


		abstract protected override TControl CreateNativeControl();

		[Internals.Preserve(Conditional = true)]
		public DatePickerRendererBase()
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			if (Control == null)
			{
				var entry = CreateNativeControl();

				entry.EditingDidBegin += OnStarted;
				entry.EditingDidEnd += OnEnded;

				_picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

				_picker.ValueChanged += HandleValueChanged;

				var width = UIScreen.MainScreen.Bounds.Width;
				var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
				var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
				var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
				{
					UpdateElementDate();
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

				entry.AccessibilityTraits = UIAccessibilityTrait.Button;

				SetNativeControl(entry);
			}

			UpdateDateFromModel(false);
			UpdateFont();
			UpdateMaximumDate();
			UpdateMinimumDate();
			UpdateTextColor();
			UpdateCharacterSpacing();
			UpdateFlowDirection();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == DatePicker.DateProperty.PropertyName || e.PropertyName == DatePicker.FormatProperty.PropertyName)
			{
				UpdateDateFromModel(true);
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
				UpdateMinimumDate();
			else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
				UpdateMaximumDate();
			else if (e.PropertyName == DatePicker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName || e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (e.PropertyName == DatePicker.FontAttributesProperty.PropertyName ||
					 e.PropertyName == DatePicker.FontFamilyProperty.PropertyName || e.PropertyName == DatePicker.FontSizeProperty.PropertyName)
			{
				UpdateFont();
			}
		}

		void HandleValueChanged(object sender, EventArgs e)
		{
			if (Element.OnThisPlatform().UpdateMode() == UpdateMode.Immediately)
			{
				UpdateElementDate();
			}
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void UpdateDateFromModel(bool animate)
		{
			if (_picker.Date.ToDateTime().Date != Element.Date.Date)
				_picker.SetDate(Element.Date.ToNSDate(), animate);

			Control.Text = Element.Date.ToString(Element.Format);
		}

		void UpdateElementDate()
		{
			ElementController.SetValueFromRenderer(DatePicker.DateProperty, _picker.Date.ToDateTime().Date);
		}

		void UpdateFlowDirection()
		{
			(Control as UITextField).UpdateTextAlignment(Element);
		}

		protected internal virtual void UpdateFont()
		{
			Control.Font = Element.ToUIFont();
		}

		void UpdateCharacterSpacing()
		{
			var textAttr = Control.AttributedText.AddCharacterSpacing(Control.Text, Element.CharacterSpacing);

			if (textAttr != null)
				Control.AttributedText = textAttr;
		}
		void UpdateMaximumDate()
		{
			_picker.MaximumDate = Element.MaximumDate.ToNSDate();
		}

		void UpdateMinimumDate()
		{
			_picker.MinimumDate = Element.MinimumDate.ToNSDate();
		}

		protected internal virtual void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			if (textColor.IsDefault || (!Element.IsEnabled && _useLegacyColorManagement))
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToUIColor();

			// HACK This forces the color to update; there's probably a more elegant way to make this happen
			Control.Text = Control.Text;
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
					_picker.ValueChanged -= HandleValueChanged;
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
	}
}