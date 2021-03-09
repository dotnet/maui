using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	public class EditorRenderer : ViewRenderer<Editor, NSTextField>
	{
		const string NewLineSelector = "insertNewline";
		bool _disposed;
		CGSize _previousSize;

		IEditorController ElementController => Element;

		public override void Layout()
		{
			base.Layout();

			if (Element != null && _previousSize != Bounds.Size)
				SetBackground(Element.Background);

			_previousSize = Bounds.Size;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(new NSTextField { UsesSingleLineMode = false });
				Control.Cell.Scrollable = true;
				Control.Cell.Wraps = true;
				Control.Changed += HandleChanged;
				Control.EditingBegan += OnEditingBegan;
				Control.EditingEnded += OnEditingEnded;
				Control.DoCommandBySelector = (control, textView, commandSelector) =>
				{
					var result = false;
					if (commandSelector.Name.StartsWith(NewLineSelector, StringComparison.InvariantCultureIgnoreCase))
					{
						textView.InsertText(new NSString(Environment.NewLine));
						result = true;
					}
					return result;
				};
			}

			if (e.NewElement == null) return;
			UpdateText();
			UpdateFont();
			UpdateTextColor();
			UpdateEditable();
			UpdateMaxLength();
			UpdateIsReadOnly();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Editor.TextProperty.PropertyName ||
				e.PropertyName == Editor.TextTransformProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateEditable();
			else if (e.PropertyName == Editor.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Editor.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Editor.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();
			else if (e.PropertyName == Microsoft.Maui.Controls.Compatibility.InputView.IsReadOnlyProperty.PropertyName)
				UpdateIsReadOnly();
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (Control == null)
				return;

			Control.BackgroundColor = color == Color.Default ? NSColor.Clear : color.ToNSColor();

			if (color == Color.Transparent)
			{
				Control.DrawsBackground = false;
				Control.Bezeled = false;
			}
			else
			{
				Control.DrawsBackground = true;
				Control.Bezeled = true;
			}

			base.SetBackgroundColor(color);
		}

		protected override void SetBackground(Brush brush)
		{
			if (Control == null)
				return;

			var backgroundImage = this.GetBackgroundImage(brush);
			Control.BackgroundColor = backgroundImage != null ? NSColor.FromPatternImage(backgroundImage) : NSColor.Clear;

			base.SetBackground(brush);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
				{
					Control.Changed -= HandleChanged;
					Control.EditingBegan -= OnEditingBegan;
					Control.EditingEnded -= OnEditingEnded;
				}
			}
			base.Dispose(disposing);
		}

		void HandleChanged(object sender, EventArgs e)
		{
			UpdateMaxLength();

			ElementController.SetValueFromRenderer(Editor.TextProperty, Control.StringValue);
		}

		void OnEditingEnded(object sender, EventArgs eventArgs)
		{
			Element.SetValue(VisualElement.IsFocusedPropertyKey, false);
			ElementController.SendCompleted();
		}

		void OnEditingBegan(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void UpdateEditable()
		{
			Control.Editable = Element.IsEnabled;
		}

		void UpdateFont()
		{
			Control.Font = Element.ToNSFont();
		}

		void UpdateText()
		{
			var text = Element.UpdateFormsText(Element.Text, Element.TextTransform);
			if (Control.StringValue != text)
				Control.StringValue = text;
		}

		void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			Control.TextColor = textColor.IsDefault ? NSColor.Black : textColor.ToNSColor();
		}

		void UpdateMaxLength()
		{
			var currentControlText = Control?.StringValue;

			if (currentControlText.Length > Element?.MaxLength)
				Control.StringValue = currentControlText.Substring(0, Element.MaxLength);
		}

		void UpdateIsReadOnly()
		{
			Control.Editable = !Element.IsReadOnly;
			if (Element.IsReadOnly && Control.Window?.FirstResponder == Control.CurrentEditor)
				Control.Window?.MakeFirstResponder(null);
		}
	}
}