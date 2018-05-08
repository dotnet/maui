using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.WindowsSpecific.InputView;

namespace Xamarin.Forms.Platform.UWP
{
	public class EditorRenderer : ViewRenderer<Editor, FormsTextBox>
	{
		private static FormsTextBox _copyOfTextBox;
		static Windows.Foundation.Size _zeroSize = new Windows.Foundation.Size(0, 0);
		bool _fontApplied;
		Brush _backgroundColorFocusedDefaultBrush;
		Brush _textDefaultBrush;
		Brush _defaultTextColorFocusBrush;
		Brush _defaultPlaceholderColorFocusBrush;
		Brush _placeholderDefaultBrush;

		IEditorController ElementController => Element;


		FormsTextBox CreateTextBox()
		{
			return new FormsTextBox
			{
				AcceptsReturn = true,
				TextWrapping = TextWrapping.Wrap,
				Style = Windows.UI.Xaml.Application.Current.Resources["FormsTextBoxStyle"] as Windows.UI.Xaml.Style
			};
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var textBox = CreateTextBox();

					SetNativeControl(textBox);

					textBox.TextChanged += OnNativeTextChanged;
					textBox.LostFocus += OnLostFocus;

					// If the Forms VisualStateManager is in play or the user wants to disable the Forms legacy
					// color stuff, then the underlying textbox should just use the Forms VSM states
					textBox.UseFormsVsm = e.NewElement.HasVisualStateGroups()
						|| !e.NewElement.OnThisPlatform().GetIsLegacyColorModeEnabled();
				}

				UpdateText();
				UpdateInputScope();
				UpdateTextColor();
				UpdateFont();
				UpdateTextAlignment();
				UpdateFlowDirection();
				UpdateMaxLength();
				UpdateDetectReadingOrderFromContent();
				UpdatePlaceholderText();
				UpdatePlaceholderColor();
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.TextChanged -= OnNativeTextChanged;
				Control.LostFocus -= OnLostFocus;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Editor.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == InputView.KeyboardProperty.PropertyName)
			{
				UpdateInputScope();
			}
			else if (e.PropertyName == InputView.IsSpellCheckEnabledProperty.PropertyName)
			{
				UpdateInputScope();
			}
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Editor.TextProperty.PropertyName)
			{
				UpdateText();
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateTextAlignment();
				UpdateFlowDirection();
			}
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == Specifics.DetectReadingOrderFromContentProperty.PropertyName)
				UpdateDetectReadingOrderFromContent();
			else if (e.PropertyName == Editor.PlaceholderProperty.PropertyName)
				UpdatePlaceholderText();
			else if (e.PropertyName == Editor.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
		}

		void OnLostFocus(object sender, RoutedEventArgs e)
		{
			ElementController.SendCompleted();
		}

		void UpdatePlaceholderText()
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
			Element.SetValueCore(Editor.TextProperty, Control.Text);
		}

		/*
		 * Purely invalidating the layout as text is added to the TextBox will not cause it to expand.
		 * If the TextBox is set to WordWrap and it is part of the layout it will refuse to Measure itself beyond its established width.
		 * Even giving it infinite constraints will cause it to always set its DesiredSize to the same width but with a vertical growth.
		 * The only way I was able to grow it was by setting layout renderers width explicitly to some value but then it just set its own Width to that Width which is not helpful.
		 * Even vertically it would measure oddly in cases of rapid text changes.
		 * Holding down the backspace key or enter key would cause the final result to be not quite right.
		 * Both of these issues were fixed by just creating a static TextBox that is not part of the layout which let me just measure
		 * the size of the text as it would fit into the TextBox unconstrained and then just return that Size from the GetDesiredSize call.
		 * */
		Size GetCopyOfSize(FormsTextBox control, Windows.Foundation.Size constraint)
		{
			if (_copyOfTextBox == null)
			{
				_copyOfTextBox = CreateTextBox();

				// This causes the copy to be initially setup correctly. 
				// I found that if the first measure of this copy occurs with Text then it will just keep defaulting to a measure with no text.
				_copyOfTextBox.Measure(_zeroSize);
			}

			_copyOfTextBox.Text = control.Text;
			_copyOfTextBox.FontSize = control.FontSize;
			_copyOfTextBox.FontFamily = control.FontFamily;
			_copyOfTextBox.FontStretch = control.FontStretch;
			_copyOfTextBox.FontStyle = control.FontStyle;
			_copyOfTextBox.FontWeight = control.FontWeight;
			_copyOfTextBox.Margin = control.Margin;
			_copyOfTextBox.Padding = control.Padding;

			// have to reset the measure to zero before it will re-measure itself
			_copyOfTextBox.Measure(_zeroSize);
			_copyOfTextBox.Measure(constraint);

			Size result = new Size
			(
				Math.Ceiling(_copyOfTextBox.DesiredSize.Width),
				Math.Ceiling(_copyOfTextBox.DesiredSize.Height)
			);

			return result;
		}


		SizeRequest CalculateDesiredSizes(FormsTextBox control, Windows.Foundation.Size constraint, EditorAutoSizeOption sizeOption)
		{
			if (sizeOption == EditorAutoSizeOption.TextChanges)
			{
				Size result = GetCopyOfSize(control, constraint);
				control.Measure(constraint);
				return new SizeRequest(result);
			}
			else
			{
				control.Measure(constraint);
				Size result = new Size(Math.Ceiling(control.DesiredSize.Width), Math.Ceiling(control.DesiredSize.Height));
				return new SizeRequest(result);
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			FormsTextBox child = Control;

			if (Children.Count == 0 || child == null)
				return new SizeRequest();

			return CalculateDesiredSizes(child, new Windows.Foundation.Size(widthConstraint, heightConstraint), Element.AutoSize);
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			Editor editor = Element;

			if (editor == null)
				return;

			bool editorIsDefault = editor.FontFamily == null &&
								   editor.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Editor), true) &&
								   editor.FontAttributes == FontAttributes.None;

			if (editorIsDefault && !_fontApplied)
				return;

			if (editorIsDefault)
			{
				// ReSharper disable AccessToStaticMemberViaDerivedType
				// Resharper wants to simplify 'TextBox' to 'Control', but then it'll conflict with the property 'Control'
				Control.ClearValue(TextBox.FontStyleProperty);
				Control.ClearValue(TextBox.FontSizeProperty);
				Control.ClearValue(TextBox.FontFamilyProperty);
				Control.ClearValue(TextBox.FontWeightProperty);
				Control.ClearValue(TextBox.FontStretchProperty);
				// ReSharper restore AccessToStaticMemberViaDerivedType
			}
			else
			{
				Control.ApplyFont(editor);
			}

			_fontApplied = true;
		}

		void UpdateInputScope()
		{
			Editor editor = Element;
			var custom = editor.Keyboard as CustomKeyboard;
			if (custom != null)
			{
				Control.IsTextPredictionEnabled = (custom.Flags & KeyboardFlags.Suggestions) != 0;
				Control.IsSpellCheckEnabled = (custom.Flags & KeyboardFlags.Spellcheck) != 0;
			}
			else
			{
				Control.ClearValue(TextBox.IsTextPredictionEnabledProperty);
				if (editor.IsSet(InputView.IsSpellCheckEnabledProperty))
					Control.IsSpellCheckEnabled = editor.IsSpellCheckEnabled;
				else
					Control.ClearValue(TextBox.IsSpellCheckEnabledProperty);
			}

			Control.InputScope = editor.Keyboard.ToInputScope();
		}

		void UpdateText()
		{
			string newText = Element.Text ?? "";

			if (Control.Text == newText)
			{
				return;
			}

			Control.Text = newText;
			Control.SelectionStart = Control.Text.Length;
		}

		void UpdateTextAlignment()
		{
			Control.UpdateTextAlignment(Element);
		}

		void UpdateTextColor()
		{
			Color textColor = Element.TextColor;

			BrushHelpers.UpdateColor(textColor, ref _textDefaultBrush,
				() => Control.Foreground, brush => Control.Foreground = brush);

			BrushHelpers.UpdateColor(textColor, ref _defaultTextColorFocusBrush,
				() => Control.ForegroundFocusBrush, brush => Control.ForegroundFocusBrush = brush);
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		void UpdateMaxLength()
		{
			Control.MaxLength = Element.MaxLength;

			var currentControlText = Control.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
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