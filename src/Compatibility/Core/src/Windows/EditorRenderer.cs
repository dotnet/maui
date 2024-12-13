using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Text;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.InputView;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class EditorRenderer : ViewRenderer<Editor, FormsTextBox>
	{
		bool _fontApplied;
		WBrush _backgroundColorFocusedDefaultBrush;
		WBrush _textDefaultBrush;
		WBrush _defaultTextColorFocusBrush;
		WBrush _defaultPlaceholderColorFocusBrush;
		WBrush _placeholderDefaultBrush;
		string _transformedText;

		IEditorController ElementController => Element;


		FormsTextBox CreateTextBox()
		{
			return new FormsTextBox
			{
				AcceptsReturn = true,
				TextWrapping = TextWrapping.Wrap,
				Style = Microsoft.UI.Xaml.Application.Current.Resources["FormsTextBoxStyle"] as Microsoft.UI.Xaml.Style,
				VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
				UpdateVerticalAlignmentOnLoad = false
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

					// The default is DetectFromContent, which we don't want because it can
					// override the FlowDirection settings. 
					textBox.TextAlignment = Microsoft.UI.Xaml.TextAlignment.Left;
				}

				UpdateText();
				UpdateInputScope();
				UpdateTextColor();
				UpdateBackground();
				UpdateCharacterSpacing();
				UpdateFont();
				UpdateFlowDirection();
				UpdateMaxLength();
				UpdateDetectReadingOrderFromContent();
				UpdatePlaceholderText();
				UpdatePlaceholderColor();
				UpdateIsReadOnly();
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
			else if (e.PropertyName == Editor.IsTextPredictionEnabledProperty.PropertyName)
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
			else if (e.IsOneOf(Editor.TextProperty, Editor.TextTransformProperty))
			{
				UpdateText();
			}
			else if (e.PropertyName == Editor.CharacterSpacingProperty.PropertyName)
			{
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
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
			else if (e.PropertyName == InputView.IsReadOnlyProperty.PropertyName)
				UpdateIsReadOnly();
		}

		protected override void UpdateBackground()
		{
			base.UpdateBackground();

			if (Control == null)
			{
				return;
			}

			BrushHelpers.UpdateBrush(Element.Background, ref _backgroundColorFocusedDefaultBrush,
			   () => Control.BackgroundFocusBrush, brush => Control.BackgroundFocusBrush = brush);
		}

		[PortHandler]
		void OnLostFocus(object sender, RoutedEventArgs e)
		{
			ElementController.SendCompleted();
		}

		[PortHandler]
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

		void OnNativeTextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs args)
		{
			_transformedText = Element.UpdateFormsText(Control.Text, Element.TextTransform);
			Element.SetValueCore(Editor.TextProperty, _transformedText);
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			FormsTextBox child = Control;

			if (Children.Count == 0 || child == null)
				return new SizeRequest();

			var constraint = new global::Windows.Foundation.Size(widthConstraint, heightConstraint);
			child.Measure(constraint);
			var result = FormsTextBox.GetCopyOfSize(child, constraint);
			return new SizeRequest(result);
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			Editor editor = Element;

			if (editor == null)
				return;

			bool editorIsDefault = editor.FontFamily == null &&
#pragma warning disable CS0612 // Type or member is obsolete
								   editor.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Editor), true) &&
#pragma warning restore CS0612 // Type or member is obsolete
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

		[PortHandler("Partially ported")]
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
				if (editor.IsSet(Editor.IsTextPredictionEnabledProperty))
					Control.IsTextPredictionEnabled = editor.IsTextPredictionEnabled;
				else
					Control.ClearValue(TextBox.IsTextPredictionEnabledProperty);
				if (editor.IsSet(InputView.IsSpellCheckEnabledProperty))
					Control.IsSpellCheckEnabled = editor.IsSpellCheckEnabled;
				else
					Control.ClearValue(TextBox.IsSpellCheckEnabledProperty);
			}

			Control.InputScope = editor.Keyboard.ToInputScope();
		}

		void UpdateCharacterSpacing()
		{
			Control.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}

		[PortHandler]
		void UpdateText()
		{
			string newText = _transformedText = Element.UpdateFormsText(Element.Text, Element.TextTransform);

			if (Control.Text == newText)
			{
				return;
			}

			Control.Text = newText;
			Control.SelectionStart = Control.Text.Length;
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

		[PortHandler]
		void UpdateMaxLength()
		{
			Control.MaxLength = Element.MaxLength;

			var currentControlText = Control.Text;

			if (currentControlText.Length > Element.MaxLength)
				Control.Text = currentControlText.Substring(0, Element.MaxLength);
		}

		[PortHandler]
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

		[PortHandler]
		void UpdateIsReadOnly()
		{
			Control.IsReadOnly = Element.IsReadOnly;
		}
	}
}