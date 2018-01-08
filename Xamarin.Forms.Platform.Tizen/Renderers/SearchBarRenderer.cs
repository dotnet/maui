using System;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SearchBarRenderer : ViewRenderer<SearchBar, Native.SearchBar>
	{
		//TODO need to add internationalization support
		const string DefaultPlaceholderText = "Search";

		static readonly EColor s_defaultCancelButtonColor = EColor.Aqua;

		//TODO: read default platform color
		static readonly EColor s_defaultPlaceholderColor = EColor.Gray;
		static readonly EColor s_defaultTextColor = EColor.Black;
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
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.TextChanged -= OnTextChanged;
					Control.SearchButtonPressed -= OnButtonPressed;
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
				Control.TextChanged += OnTextChanged;
				Control.SearchButtonPressed += OnButtonPressed;
			}
			Control.BatchBegin();
			base.OnElementChanged(e);
			Control.BatchCommit();
		}

		protected override Size MinimumSize()
		{
			return new Size(136, 65);
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's cancel button color property.
		/// Converts current Color to ElmSharp.Color instance and sets it in the underlying Xamarin.Forms.Platform.Tizen.Native widget.
		/// </summary>
		void CancelButtonColorPropertyHandler()
		{
			Control.CancelButtonColor = Element.CancelButtonColor.IsDefault ? s_defaultCancelButtonColor : Element.CancelButtonColor.ToNative();
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
			Control.FontFamily = Element.FontFamily;
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
		void PlaceholderColorPropertyHandler()
		{
			Control.PlaceholderColor = Element.PlaceholderColor.IsDefault ? s_defaultPlaceholderColor : Element.PlaceholderColor.ToNative();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's placeholder text property.
		/// </summary>
		void PlaceholderPropertyHandler()
		{
			Control.Placeholder = Element.Placeholder == null ? DefaultPlaceholderText : Element.Placeholder;
		}

		/// <summary>
		/// Called on every change of underlying SearchBar's Text property.
		/// Rewrites current underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar's Text contents to its Xamarin counterpart.
		/// </summary>
		/// <param name="sender">Sender.</param>
		void OnTextChanged(object sender, EventArgs e)
		{
			Element.Text = Control.Text;
		}

		/// <summary>
		/// Called when the user clicks the Search button.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		void OnButtonPressed(object sender, EventArgs e)
		{
			(Element as ISearchBarController).OnSearchButtonPressed();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's text color property.
		/// Converts current TextColor property value to ElmSharp.Color instance
		/// and sets it in the underlying Xamarin.Forms.Platform.Tizen.Native.SearchBar widget.
		/// </summary>
		void TextColorPropertyHandler()
		{
			Control.TextColor = Element.TextColor.IsDefault ? s_defaultTextColor : Element.TextColor.ToNative();
		}

		/// <summary>
		/// Called upon changing of Xamarin widget's text property.
		/// </summary>
		void TextPropertyHandler()
		{
			Control.Text = Element.Text;
		}
	}
}
