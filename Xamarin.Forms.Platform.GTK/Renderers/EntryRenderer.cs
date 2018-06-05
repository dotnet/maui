using Gtk;
using Pango;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;
using Xamarin.Forms.Platform.GTK.Helpers;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
	public class EntryRenderer : ViewRenderer<Entry, EntryWrapper>
	{
		private bool _disposed;

		IEntryController EntryController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			if (Control == null)
			{
				// Custom control. The control is composed of a Gtk.Entry and a Gtk.Label to allow to display Placeholder.
				var wrapper = new EntryWrapper();
				SetNativeControl(wrapper);

				wrapper.Entry.Changed += OnChanged;
				wrapper.Entry.Focused += OnFocused;
				wrapper.Entry.EditingDone += OnEditingDone;
				wrapper.Entry.KeyReleaseEvent += OnKeyReleased;
			}

			if (e.NewElement != null)
			{
				UpdateText();
				UpdateColor();
				UpdateAlignment();
				UpdateFont();
				UpdateTextVisibility();
				UpdatePlaceholder();
				UpdateMaxLength();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Entry.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Entry.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName)
				UpdateAlignment();
			else if (e.PropertyName == Entry.FontAttributesProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontFamilyProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Entry.IsPasswordProperty.PropertyName)
				UpdateTextVisibility();
			else if (e.PropertyName == Entry.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == Entry.PlaceholderColorProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == InputView.MaxLengthProperty.PropertyName)
				UpdateMaxLength();

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
				{
					Control.Entry.Changed -= OnChanged;
					Control.Entry.Focused -= OnFocused;
					Control.Entry.EditingDone -= OnEditingDone;
					Control.Entry.KeyReleaseEvent -= OnKeyReleased;
				}
			}

			base.Dispose(disposing);
		}

		protected override void UpdateBackgroundColor()
		{
			if (!Element.BackgroundColor.IsDefaultOrTransparent())
			{
				Control.SetBackgroundColor(Element.BackgroundColor.ToGtkColor());
			}
			else
			{
				Control.SetBackgroundColor(Color.Transparent.ToGtkColor());
			}
		}

		private void UpdateText()
		{
			if (Control.Entry.Text != Element.Text)
				Control.Entry.Text = Element.Text ?? string.Empty;
		}

		private void UpdateColor()
		{
			var textColor = Element.TextColor;

			Control.SetTextColor(textColor.ToGtkColor());
		}

		private void UpdateAlignment()
		{
			Control.SetAlignment(Element.HorizontalTextAlignment.ToNativeValue());
		}

		private void UpdateFont()
		{
			FontDescription fontDescription = FontDescriptionHelper.CreateFontDescription(
				Element.FontSize, Element.FontFamily, Element.FontAttributes);
			Control.SetFont(fontDescription);
		}

		private void UpdateTextVisibility()
		{
			Control.Entry.Visibility = !Element.IsPassword;
		}

		private void UpdatePlaceholder()
		{
			Control.PlaceholderText = Element.Placeholder ?? string.Empty;
			Control.SetPlaceholderTextColor(Element.PlaceholderColor.ToGtkColor());
		}

		private void OnChanged(object sender, System.EventArgs e)
		{
			ElementController.SetValueFromRenderer(Entry.TextProperty, Control.Entry.Text);
		}

		private void OnFocused(object o, FocusedArgs args)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		private void OnEditingDone(object sender, System.EventArgs e)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
			EntryController?.SendCompleted();
		}

		private void OnKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Return)
			{
				EntryController.SendCompleted();
			}
		}

		private void UpdateMaxLength()
		{
			Control.SetMaxLength(Element.MaxLength);
		}
	}
}