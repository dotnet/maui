using System.ComponentModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.WindowsSpecific.InputView;

namespace Xamarin.Forms.Platform.UWP
{
	public class EntryRenderer : ViewRenderer<Entry, FormsTextBox>
	{
		bool _fontApplied;
		Brush _backgroundColorFocusedDefaultBrush;
		Brush _placeholderDefaultBrush;
		Brush _textDefaultBrush;
		Brush _defaultTextColorFocusBrush;
		Brush _defaultPlaceholderColorFocusBrush;

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var textBox = new FormsTextBox { Style = Windows.UI.Xaml.Application.Current.Resources["FormsTextBoxStyle"] as Windows.UI.Xaml.Style };

					SetNativeControl(textBox);
					textBox.TextChanged += OnNativeTextChanged;
					textBox.KeyUp += TextBoxOnKeyUp;

					// If the Forms VisualStateManager is in play or the user wants to disable the Forms legacy
					// color stuff, then the underlying textbox should just use the Forms VSM states
					textBox.UseFormsVsm = e.NewElement.HasVisualStateGroups()
						|| !e.NewElement.OnThisPlatform().GetIsLegacyColorModeEnabled();
				}

				UpdateIsPassword();
				UpdateText();
				UpdatePlaceholder();
				UpdateTextColor();
				UpdateFont();
				UpdateInputScope();
				UpdateAlignment();
				UpdatePlaceholderColor();
				UpdateDetectReadingOrderFromContent();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.TextChanged -= OnNativeTextChanged;
				Control.KeyUp -= TextBoxOnKeyUp;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Entry.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdateIsPassword();
			else if (e.PropertyName == Entry.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
				UpdateInputScope();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Specifics.DetectReadingOrderFromContentProperty.PropertyName)
				UpdateDetectReadingOrderFromContent();
		}

		protected override void UpdateBackgroundColor()
		{
			base.UpdateBackgroundColor();

			if (Control == null)
			{
				return;
			}

			// By default some platforms have alternate default background colors when focused
			BrushHelpers.UpdateColor(Element.BackgroundColor, ref _backgroundColorFocusedDefaultBrush, 
				() => Control.BackgroundFocusBrush, brush => Control.BackgroundFocusBrush = brush);
		}

		void OnNativeTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs args)
		{
			Element.SetValueCore(Entry.TextProperty, Control.Text);
		}

		void TextBoxOnKeyUp(object sender, KeyRoutedEventArgs args)
		{
			if (args?.Key != VirtualKey.Enter)
				return;


			// Hide the soft keyboard; this matches the behavior of Forms on Android/iOS
			Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();

			((IEntryController)Element).SendCompleted();
		}

		void UpdateAlignment()
		{
			Control.TextAlignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			Entry entry = Element;

			if (entry == null)
				return;

			bool entryIsDefault = entry.FontFamily == null && entry.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Entry), true) && entry.FontAttributes == FontAttributes.None;

			if (entryIsDefault && !_fontApplied)
				return;

			if (entryIsDefault)
			{
				// ReSharper disable AccessToStaticMemberViaDerivedType
				// Resharper wants to simplify 'FormsTextBox' to 'Control', but then it'll conflict with the property 'Control'
				Control.ClearValue(FormsTextBox.FontStyleProperty);
				Control.ClearValue(FormsTextBox.FontSizeProperty);
				Control.ClearValue(FormsTextBox.FontFamilyProperty);
				Control.ClearValue(FormsTextBox.FontWeightProperty);
				Control.ClearValue(FormsTextBox.FontStretchProperty);
				// ReSharper restore AccessToStaticMemberViaDerivedType
			}
			else
			{
				Control.ApplyFont(entry);
			}

			_fontApplied = true;
		}

		void UpdateInputScope()
		{
			var custom = Element.Keyboard as CustomKeyboard;
			if (custom != null)
			{
				Control.IsTextPredictionEnabled = (custom.Flags & KeyboardFlags.Suggestions) != 0;
				Control.IsSpellCheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) != 0;
			}

			Control.InputScope = Element.Keyboard.ToInputScope();
		}

		void UpdateIsPassword()
		{
			Control.IsPassword = Element.IsPassword;
		}

		void UpdatePlaceholder()
		{
			Control.PlaceholderText = Element.Placeholder ?? "";
		}

		void UpdatePlaceholderColor()
		{
			Color placeholderColor = Element.PlaceholderColor;

			BrushHelpers.UpdateColor(placeholderColor, ref _placeholderDefaultBrush, 
				() => Control.PlaceholderForegroundBrush, brush => Control.PlaceholderForegroundBrush = brush);

			BrushHelpers.UpdateColor(placeholderColor, ref _defaultPlaceholderColorFocusBrush, 
				() => Control.PlaceholderForegroundFocusBrush, brush => Control.PlaceholderForegroundFocusBrush = brush);
		}

		void UpdateText()
		{
			Control.Text = Element.Text ?? "";
		}

		void UpdateTextColor()
		{
			Color textColor = Element.TextColor;

			BrushHelpers.UpdateColor(textColor, ref _textDefaultBrush,
				() => Control.Foreground, brush => Control.Foreground = brush);

			BrushHelpers.UpdateColor(textColor, ref _defaultTextColorFocusBrush,
				() => Control.ForegroundFocusBrush, brush => Control.ForegroundFocusBrush = brush);
		}

		void UpdateDetectReadingOrderFromContent()
		{
			if (Element.IsSet(Specifics.DetectReadingOrderFromContentProperty))
			{
				if (Element.OnThisPlatform().GetDetectReadingOrderFromContent())
				{
					Control.TextReadingOrder = TextReadingOrder.DetectFromContent;
				}
				else
				{
					Control.TextReadingOrder = TextReadingOrder.UseFlowDirection;
				}
			}
		}
	}
}