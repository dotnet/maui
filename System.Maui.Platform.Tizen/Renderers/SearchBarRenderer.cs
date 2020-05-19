using System;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, Native.SearchBar>
	{

		/// <summary>
		/// Creates a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.SearchBarRenderer"/> class.
		/// Registers handlers for various properties of the SearchBar widget.
		/// </summary>
		public SearchBarRenderer()
		{
			RegisterPropertyHandler(SearchBar.CancelButtonColorProperty, CancelButtonColorPropertyHandler);
			RegisterPropertyHandler(SearchBar.FontAttributesProperty, FontAttributesPropertyHandler);
			RegisterPropertyHandler(SearchBar.FontFamilyProperty, FontFamilyPropertyHandler);
			RegisterPropertyHandler(SearchBar.FontSizeProperty, FontSizePropertyHandler);
			RegisterPropertyHandler(SearchBar.HorizontalTextAlignmentProperty, HorizontalTextAlignmentPropertyHandler);
			RegisterPropertyHandler(SearchBar.PlaceholderProperty, PlaceholderPropertyHandler);
			RegisterPropertyHandler(SearchBar.PlaceholderColorProperty, PlaceholderColorPropertyHandler);
			RegisterPropertyHandler(SearchBar.TextProperty, TextPropertyHandler);
			RegisterPropertyHandler(SearchBar.TextColorProperty, TextColorPropertyHandler);
			RegisterPropertyHandler(InputView.KeyboardProperty, UpdateKeyboard);
			RegisterPropertyHandler(InputView.MaxLengthProperty, UpdateMaxLength);
			RegisterPropertyHandler(InputView.IsSpellCheckEnabledProperty, UpdateIsSpellCheckEnabled);
			RegisterPropertyHandler(InputView.IsReadOnlyProperty, UpdateIsReadOnly);
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.TextChanged -= OnTextChanged;
					Control.Activated -= OnActivated;
				}
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// A method called whenever the associated element has changed.
		/// </summary>
		protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.SearchBar(Forms.NativeParent));
				Control.IsSingleLine = true;
				Control.SetInputPanelReturnKeyType(ElmSharp.InputPanelReturnKeyType.Search);

				Control.TextChanged += OnTextChanged;
				Control.Activated += OnActivated;
				Control.PrependMarkUpFilter(MaxLengthFilter);

			}
			base.OnElementChanged(e);
		}

		protected override Size MinimumSize()
		{
			return Control.Measure(Control.MinimumWidth, Control.MinimumHeight).ToDP();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's cancel button color property.
		/// Converts current Color to ElmSharp.Color instance and sets it in the underlying Xamarin.Forms.Platform.Tizen.Native widget.
		/// </summary>
		void CancelButtonColorPropertyHandler(bool initialize)
		{
			if (initialize && Element.CancelButtonColor.IsDefault)
				return;

			Control.SetClearButtonColor(Element.CancelButtonColor.ToNative());
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's font attributes property.
		/// Converts current FontAttributes to ElmSharp.FontAttributes instance
		/// and sets it in the underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar widget.
		/// </summary>
		void FontAttributesPropertyHandler()
		{
			Control.FontAttributes = Element.FontAttributes;
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's font family property.
		/// Sets current value of FontFamily property to the underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar widget.
		/// </summary>
		void FontFamilyPropertyHandler()
		{
			Control.FontFamily = Element.FontFamily.ToNativeFontFamily();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's font size property.
		/// Sets current value of FontSize property to the underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar widget.
		/// </summary>
		void FontSizePropertyHandler()
		{
			Control.FontSize = Element.FontSize;
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's horizontal text alignment property.
		/// Converts current HorizontalTextAlignment property's value to Xamarin.Forms.Platform.Tizen.Native.TextAlignment instance
		/// and sets it in the underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar widget.
		/// </summary>
		void HorizontalTextAlignmentPropertyHandler()
		{
			Control.HorizontalTextAlignment = Element.HorizontalTextAlignment.ToNative();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's placeholder color property.
		/// Converts current PlaceholderColor property value to ElmSharp.Color instance
		/// and sets it in the underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar widget.
		/// </summary>
		void PlaceholderColorPropertyHandler(bool initialize)
		{
			if (initialize && Element.TextColor.IsDefault)
				return;

			Control.PlaceholderColor = Element.PlaceholderColor.ToNative();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's placeholder text property.
		/// </summary>
		void PlaceholderPropertyHandler()
		{
			Control.Placeholder = Element.Placeholder;
		}

		/// <summary>
		/// Called on every change of underlying SearchBar's Text property.
		/// Rewrites current underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar's Text contents to its Xamarin counterpart.
		/// </summary>
		/// <param name="sender">Sender.</param>
		void OnTextChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(SearchBar.TextProperty, Control.Text);
		}

		/// <summary>
		/// Called when the user clicks the Search button.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		void OnActivated(object sender, EventArgs e)
		{
			Control.HideInputPanel();
			(Element as ISearchBarController).OnSearchButtonPressed();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's text color property.
		/// Converts current TextColor property value to ElmSharp.Color instance
		/// and sets it in the underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar widget.
		/// </summary>
		void TextColorPropertyHandler(bool initialize)
		{
			if (initialize && Element.TextColor.IsDefault)
				return;

			Control.TextColor = Element.TextColor.ToNative();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's text property.
		/// </summary>
		void TextPropertyHandler()
		{
			Control.Text = Element.Text;
		}

		void UpdateKeyboard(bool initialize)
		{
			if (initialize && Element.Keyboard == Keyboard.Default)
				return;
			Control.UpdateKeyboard(Element.Keyboard, Element.IsSpellCheckEnabled, true);
		}

		void UpdateIsSpellCheckEnabled()
		{
			Control.InputHint = Element.Keyboard.ToInputHints(Element.IsSpellCheckEnabled, true);
		}

		void UpdateMaxLength()
		{
			if (Control.Text.Length > Element.MaxLength)
				Control.Text = Control.Text.Substring(0, Element.MaxLength);
		}

		string MaxLengthFilter(ElmSharp.Entry entry, string s)
		{
			if (entry.Text.Length < Element.MaxLength)
				return s;

			return null;
		}

		void UpdateIsReadOnly()
		{
			Control.IsEditable = !Element.IsReadOnly;
		}
	}
}
