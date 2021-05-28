using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class EditorRenderer : EditorRendererBase<UITextView>
	{
		// Using same placeholder color as for the Entry
		readonly UIColor _defaultPlaceholderColor = ColorExtensions.PlaceholderColor;

		UILabel _placeholderLabel;

		[Preserve(Conditional = true)]
		public EditorRenderer()
		{
			Frame = new CGRect(0, 20, 320, 40);
		}

		[PortHandler]
		protected override UITextView CreateNativeControl()
		{
			return new FormsUITextView(CGRect.Empty);
		}

		protected override UITextView TextView => Control;

		protected internal override void UpdateText()
		{
			base.UpdateText();
			_placeholderLabel.Hidden = !string.IsNullOrEmpty(TextView.Text);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			bool initializing = false;
			if (e.NewElement != null && _placeholderLabel == null)
			{
				initializing = true;
				// create label so it can get updated during the initial setup loop
				_placeholderLabel = new UILabel
				{
					BackgroundColor = UIColor.Clear,
					Frame = new CGRect(0, 0, Frame.Width, Frame.Height),
					Lines = 0
				};
			}

			base.OnElementChanged(e);

			if (e.NewElement != null && initializing)
			{
				CreatePlaceholderLabel();
			}
		}

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			_placeholderLabel.Font = Element.ToUIFont();
		}

		[PortHandler]
		protected internal override void UpdatePlaceholderText()
		{
			_placeholderLabel.Text = Element.Placeholder;

			_placeholderLabel.SizeToFit();
		}

		[PortHandler("Partially ported")]
		protected internal override void UpdateCharacterSpacing()
		{
			var textAttr = TextView.AttributedText.WithCharacterSpacing(Element.CharacterSpacing);

			if (textAttr != null)
				TextView.AttributedText = textAttr;

			var placeHolder = _placeholderLabel.AttributedText.WithCharacterSpacing(Element.CharacterSpacing);

			if (placeHolder != null)
				_placeholderLabel.AttributedText = placeHolder;
		}

		[PortHandler]
		protected internal override void UpdatePlaceholderColor()
		{
			Color placeholderColor = Element.PlaceholderColor;
			_placeholderLabel.TextColor = placeholderColor?.ToUIColor() ?? _defaultPlaceholderColor;
		}

		[PortHandler]
		void CreatePlaceholderLabel()
		{
			if (Control == null)
			{
				return;
			}

			Control.AddSubview(_placeholderLabel);

			var edgeInsets = TextView.TextContainerInset;
			var lineFragmentPadding = TextView.TextContainer.LineFragmentPadding;

			var vConstraints = NSLayoutConstraint.FromVisualFormat(
				"V:|-" + edgeInsets.Top + "-[_placeholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("_placeholderLabel") })
			);

			var hConstraints = NSLayoutConstraint.FromVisualFormat(
				"H:|-" + lineFragmentPadding + "-[_placeholderLabel]-" + lineFragmentPadding + "-|",
				0, new NSDictionary(),
				NSDictionary.FromObjectsAndKeys(
					new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("_placeholderLabel") })
			);

			_placeholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;
			_placeholderLabel.AttributedText = _placeholderLabel.AttributedText.WithCharacterSpacing(Element.CharacterSpacing);

			Control.AddConstraints(hConstraints);
			Control.AddConstraints(vConstraints);
		}

	}

	public abstract class EditorRendererBase<TControl> : ViewRenderer<Editor, TControl>
		where TControl : UIView
	{
		bool _disposed;
		IUITextViewDelegate _pleaseDontCollectMeGarbageCollector;

		IEditorController ElementController => Element;
		protected abstract UITextView TextView { get; }

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (Control != null)
				{
					TextView.Changed -= HandleChanged;
					TextView.Started -= OnStarted;
					TextView.Ended -= OnEnded;
					TextView.ShouldChangeText -= ShouldChangeText;
					if (Control is IFormsUITextView formsUITextView)
						formsUITextView.FrameChanged -= OnFrameChanged;
				}
			}

			_pleaseDontCollectMeGarbageCollector = null;
			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement == null)
				return;

			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());

				if (Device.Idiom == TargetIdiom.Phone)
				{
					// iPhone does not have a dismiss keyboard button
					var keyboardWidth = UIScreen.MainScreen.Bounds.Width;
					var accessoryView = new UIToolbar(new CGRect(0, 0, keyboardWidth, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };

					var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
					var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
					{
						TextView.ResignFirstResponder();
						ElementController.SendCompleted();
					});
					accessoryView.SetItems(new[] { spacer, doneButton }, false);
					TextView.InputAccessoryView = accessoryView;
				}

				TextView.Changed += HandleChanged;
				TextView.Started += OnStarted;
				TextView.Ended += OnEnded;
				TextView.ShouldChangeText += ShouldChangeText;
				_pleaseDontCollectMeGarbageCollector = TextView.Delegate;
			}

			UpdateFont();
			UpdatePlaceholderText();
			UpdatePlaceholderColor();
			UpdateTextColor();
			UpdateText();
			UpdateCharacterSpacing();
			UpdateKeyboard();
			UpdateEditable();
			UpdateTextAlignment();
			UpdateMaxLength();
			UpdateAutoSizeOption();
			UpdateReadOnly();
			UpdateUserInteraction();
		}

		protected internal virtual void UpdateAutoSizeOption()
		{
			if (Control is IFormsUITextView textView)
			{
				textView.FrameChanged -= OnFrameChanged;
				if (Element.AutoSize == EditorAutoSizeOption.TextChanges)
					textView.FrameChanged += OnFrameChanged;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.IsOneOf(Editor.TextProperty, Editor.TextTransformProperty))
			{
				UpdateText();
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == Microsoft.Maui.Controls.InputView.KeyboardProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == Microsoft.Maui.Controls.InputView.IsSpellCheckEnabledProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == Editor.IsTextPredictionEnabledProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName || e.PropertyName == Microsoft.Maui.Controls.InputView.IsReadOnlyProperty.PropertyName)
				UpdateUserInteraction();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateTextAlignment();
			else if (e.PropertyName == Microsoft.Maui.Controls.InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == Editor.PlaceholderProperty.PropertyName)
			{
				UpdatePlaceholderText();
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == Editor.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholderColor();
			else if (e.PropertyName == Editor.AutoSizeProperty.PropertyName)
				UpdateAutoSizeOption();
		}

		void HandleChanged(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(Editor.TextProperty, TextView.Text);
		}

		private void OnFrameChanged(object sender, EventArgs e)
		{
			// When a new line is added to the UITextView the resize happens after the view has already scrolled
			// This causes the view to reposition without the scroll. If TextChanges is enabled then the Frame
			// will resize until it can't anymore and thus it should never be scrolled until the Frame can't increase in size
			if (Element.AutoSize == EditorAutoSizeOption.TextChanges)
			{
				TextView.ScrollRangeToVisible(new NSRange(0, 0));
			}
		}

		[PortHandler("Missing to port the code related with Focus")]
		void OnEnded(object sender, EventArgs eventArgs)
		{
			if (TextView.Text != Element.Text)
				ElementController.SetValueFromRenderer(Editor.TextProperty, TextView.Text);

			Element.SetValue(VisualElement.IsFocusedPropertyKey, false);
			ElementController.SendCompleted();
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void UpdateEditable()
		{
			TextView.Editable = Element.IsEnabled;
			TextView.UserInteractionEnabled = Element.IsEnabled;

			if (TextView.InputAccessoryView != null)
				TextView.InputAccessoryView.Hidden = !Element.IsEnabled;
		}

		[PortHandler]
		protected internal virtual void UpdateFont()
		{
			var font = Element.ToUIFont();
			TextView.Font = font;
		}

		[PortHandler("Partially Ported")]
		void UpdateKeyboard()
		{
			var keyboard = Element.Keyboard;
			TextView.ApplyKeyboard(keyboard);
			if (!(keyboard is CustomKeyboard))
			{
				if (Element.IsSet(Microsoft.Maui.Controls.InputView.IsSpellCheckEnabledProperty))
				{
					if (!Element.IsSpellCheckEnabled)
					{
						TextView.SpellCheckingType = UITextSpellCheckingType.No;
					}
				}
				if (Element.IsSet(Editor.IsTextPredictionEnabledProperty))
				{
					if (!Element.IsTextPredictionEnabled)
					{
						TextView.AutocorrectionType = UITextAutocorrectionType.No;
					}
				}
			}
			TextView.ReloadInputViews();
		}

		[PortHandler]
		protected internal virtual void UpdateText()
		{
			var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
			if (TextView.Text != text)
			{
				TextView.Text = text;
			}
		}

		[PortHandler]
		protected internal abstract void UpdatePlaceholderText();

		[PortHandler]
		protected internal abstract void UpdatePlaceholderColor();
		protected internal abstract void UpdateCharacterSpacing();

		void UpdateTextAlignment()
		{
			TextView.UpdateTextAlignment(Element);
		}

		[PortHandler]
		protected internal virtual void UpdateTextColor()
			=> TextView.TextColor = Element.TextColor?.ToUIColor() ?? ColorExtensions.LabelColor;

		[PortHandler]
		void UpdateMaxLength()
		{
			var currentControlText = TextView.Text;

			if (currentControlText.Length > Element.MaxLength)
				TextView.Text = currentControlText.Substring(0, Element.MaxLength);
		}

		[PortHandler]
		protected virtual bool ShouldChangeText(UITextView textView, NSRange range, string text)
		{
			var newLength = textView.Text.Length + text.Length - range.Length;
			return newLength <= Element.MaxLength;
		}

		[PortHandler]
		void UpdateReadOnly()
		{
			TextView.UserInteractionEnabled = !Element.IsReadOnly;

			// Control and TextView might be different
			Control.UserInteractionEnabled = !Element.IsReadOnly;
		}

		void UpdateUserInteraction()
		{
			if (Element.IsEnabled && Element.IsReadOnly)
				UpdateReadOnly();
			else
				UpdateEditable();
		}

		internal class FormsUITextView : UITextView, IFormsUITextView
		{
			public event EventHandler FrameChanged;

			public FormsUITextView(CGRect frame) : base(frame)
			{
			}


			public override CGRect Frame
			{
				get => base.Frame;
				set
				{
					base.Frame = value;
					FrameChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}
	}

	internal interface IFormsUITextView
	{
		event EventHandler FrameChanged;
	}
}