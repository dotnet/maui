using System;
using System.ComponentModel;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class EditorRenderer : ViewRenderer<Editor, NSTextField>
	{
		const string NewLineSelector = "insertNewline";
		bool _disposed;

		IEditorController ElementController => Element;

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
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Editor.TextProperty.PropertyName)
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
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (Control == null)
				return;

			Control.BackgroundColor = color == Color.Default ? NSColor.Clear : color.ToNSColor();

			base.SetBackgroundColor(color);
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
			if (Control.StringValue != Element.Text)
				Control.StringValue = Element.Text ?? string.Empty;
		}

		void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			Control.TextColor = textColor.IsDefault ? NSColor.Black : textColor.ToNSColor();
		}
	}
}