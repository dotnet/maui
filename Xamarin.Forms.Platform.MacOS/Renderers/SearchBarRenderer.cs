using System;
using System.ComponentModel;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, NSSearchField>
	{
		NSColor _defaultTextColor;

		IElementController ElementController => Element;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.Changed -= OnTextChanged;
					Control.Cell.CancelButtonCell.Activated -= OnCancelClicked;
					Control.Cell.SearchButtonCell.Activated -= OnSearchButtonClicked;
					Control.EditingEnded -= OnEditingEnded;
					Control.EditingBegan -= OnEditingStarted;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new NSSearchField { BackgroundColor = NSColor.Clear, DrawsBackground = false });

					Control.Cell.CancelButtonCell.Activated += OnCancelClicked;
					Control.Cell.SearchButtonCell.Activated += OnSearchButtonClicked;

					Control.Changed += OnTextChanged;
					Control.EditingBegan += OnEditingStarted;
					Control.EditingEnded += OnEditingEnded;
				}

				UpdatePlaceholder();
				UpdateText();
				UpdateFont();
				UpdateIsEnabled();
				UpdateCancelButton();
				UpdateAlignment();
				UpdateTextColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == SearchBar.PlaceholderProperty.PropertyName ||
				e.PropertyName == SearchBar.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
				UpdateTextColor();
				UpdatePlaceholder();
			}
			else if (e.PropertyName == SearchBar.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == SearchBar.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == SearchBar.CancelButtonColorProperty.PropertyName)
				UpdateCancelButton();
			else if (e.PropertyName == SearchBar.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == SearchBar.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlignment();
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null)
				return;
			Control.BackgroundColor = color == Color.Default ? NSColor.Clear : color.ToNSColor();

			UpdateCancelButton();
		}

		void OnCancelClicked(object sender, EventArgs args)
		{
			ElementController.SetValueFromRenderer(SearchBar.TextProperty, null);
			Control.ResignFirstResponder();
		}

		void OnEditingEnded(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnEditingStarted(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnSearchButtonClicked(object sender, EventArgs e)
		{
			Element.OnSearchButtonPressed();
			Control.ResignFirstResponder();
		}

		void OnTextChanged(object sender, EventArgs a)
		{
			ElementController.SetValueFromRenderer(SearchBar.TextProperty, Control.StringValue);
		}

		void UpdateAlignment()
		{
			Control.Alignment = Element.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
		}

		void UpdateCancelButton()
		{
			var cancelButtonColor = Element.CancelButtonColor;

			if (cancelButtonColor.IsDefault)
			{
				Control.Cell.CancelButtonCell.Title = "";
			}
			else
			{
				var textWithColor = new NSAttributedString(Control.Cell.CancelButtonCell.Title ?? "",
					foregroundColor: cancelButtonColor.ToNSColor());
				Control.Cell.CancelButtonCell.AttributedTitle = textWithColor;
			}
		}

		void UpdateFont()
		{
			Control.Font = Element.ToNSFont();
		}

		void UpdateIsEnabled()
		{
			Control.Enabled = Element.IsEnabled;
		}

		void UpdatePlaceholder()
		{
			var formatted = (FormattedString)Element.Placeholder ?? string.Empty;
			var targetColor = Element.PlaceholderColor;
			var color = Element.IsEnabled && !targetColor.IsDefault ? targetColor : ColorExtensions.SeventyPercentGrey.ToColor();
			Control.PlaceholderAttributedString = formatted.ToAttributed(Element, color);
		}

		void UpdateText()
		{
			Control.StringValue = Element.Text ?? "";
			UpdateCancelButton();
		}

		void UpdateTextColor()
		{
			_defaultTextColor = _defaultTextColor ?? Control.TextColor;
			var targetColor = Element.TextColor;

			Control.TextColor = targetColor.ToNSColor(_defaultTextColor);
		}
	}
}