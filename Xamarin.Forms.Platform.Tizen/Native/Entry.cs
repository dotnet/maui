using System;
using ElmSharp;
using EEntry = ElmSharp.Entry;
using EColor = ElmSharp.Color;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Extends the Entry control, providing basic formatting features,
	/// i.e. font color, size, placeholder.
	/// </summary>
	public class Entry : EEntry, IMeasurable, IBatchable
	{
		static readonly int s_VariationNormal = 0;
		static readonly int s_VariationSignedAndDecimal = 3;

		/// <summary>
		/// Holds the formatted text of the entry.
		/// </summary>
		readonly Span _span = new Span();

		/// <summary>
		/// Holds the formatted text of the placeholder.
		/// </summary>
		readonly Span _placeholderSpan = new Span();

		/// <summary>
		/// Helps to detect whether the text change was initiated by the user
		/// or via the Text property.
		/// </summary>
		int _changedByUserCallbackDepth;

		/// <summary>
		/// The type of the keyboard used by the entry.
		/// </summary>
		Keyboard _keyboard;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.Entry"/> class.
		/// </summary>
		/// <param name="parent">Parent evas object.</param>
		public Entry(EvasObject parent) : base(parent)
		{
			Scrollable = true;

			ChangedByUser += (s, e) =>
			{
				_changedByUserCallbackDepth++;

				Text = GetInternalText();

				_changedByUserCallbackDepth--;
			};

			ApplyKeyboard(Keyboard.Normal);
		}

		/// <summary>
		/// Occurs when the text has changed.
		/// </summary>
		public event EventHandler<TextChangedEventArgs> TextChanged;

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		public override string Text
		{
			get
			{
				return _span.Text;
			}

			set
			{
				if (value != _span.Text)
				{
					var old = _span.Text;
					_span.Text = value;
					ApplyTextAndStyle();
					Device.StartTimer(TimeSpan.FromTicks(1), () =>
					{
						OnTextChanged(old, value);
						return false;
					});
				}
			}
		}

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		/// <value>The color of the text.</value>
		public EColor TextColor
		{
			get
			{
				return _span.ForegroundColor;
			}

			set
			{
				if (!_span.ForegroundColor.Equals(value))
				{
					_span.ForegroundColor = value;
					ApplyTextAndStyle();
				}
			}
		}

		/// <summary>
		/// Gets or sets the font family of the text and the placeholder.
		/// </summary>
		/// <value>The font family of the text and the placeholder.</value>
		public string FontFamily
		{
			get
			{
				return _span.FontFamily;
			}

			set
			{
				if (value != _span.FontFamily)
				{
					_span.FontFamily = value;
					ApplyTextAndStyle();

					_placeholderSpan.FontFamily = value;
					ApplyPlaceholderAndStyle();
				}
			}
		}

		/// <summary>
		/// Gets or sets the font attributes of the text and the placeholder.
		/// </summary>
		/// <value>The font attributes of the text and the placeholder.</value>
		public FontAttributes FontAttributes
		{
			get
			{
				return _span.FontAttributes;
			}

			set
			{
				if (value != _span.FontAttributes)
				{
					_span.FontAttributes = value;
					ApplyTextAndStyle();

					_placeholderSpan.FontAttributes = value;
					ApplyPlaceholderAndStyle();
				}
			}
		}


		/// <summary>
		/// Gets or sets the size of the font of both text and placeholder.
		/// </summary>
		/// <value>The size of the font of both text and placeholder.</value>
		public double FontSize
		{
			get
			{
				return _span.FontSize;
			}

			set
			{
				if (value != _span.FontSize)
				{
					_span.FontSize = value;
					ApplyTextAndStyle();

					_placeholderSpan.FontSize = value;
					ApplyPlaceholderAndStyle();
				}
			}
		}

		/// <summary>
		/// Gets or sets the font weight for the text.
		/// </summary>
		/// <value>The weight of the font.</value>
		public string FontWeight
		{
			get
			{
				return _span.FontWeight;
			}

			set
			{
				if (value != _span.FontWeight)
				{
					_span.FontWeight = value;
					ApplyTextAndStyle();

					_placeholderSpan.FontWeight = value;
					ApplyPlaceholderAndStyle();
				}
			}
		}

		/// <summary>
		/// Gets or sets the horizontal text alignment of both text and placeholder.
		/// </summary>
		/// <value>The horizontal text alignment of both text and placeholder.</value>
		public TextAlignment HorizontalTextAlignment
		{
			get
			{
				return _span.HorizontalTextAlignment;
			}

			set
			{
				if (value != _span.HorizontalTextAlignment)
				{
					_span.HorizontalTextAlignment = value;
					ApplyTextAndStyle();

					_placeholderSpan.HorizontalTextAlignment = value;
					ApplyPlaceholderAndStyle();
				}
			}
		}

		/// <summary>
		/// Gets or sets the keyboard type used by the entry.
		/// </summary>
		/// <value>The keyboard type.</value>
		public Keyboard Keyboard
		{
			get
			{
				return _keyboard;
			}

			set
			{
				if (value != _keyboard)
				{
					ApplyKeyboard(value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the placeholder's text.
		/// </summary>
		/// <value>The placeholder's text.</value>
		public string Placeholder
		{
			get
			{
				return _placeholderSpan.Text;
			}

			set
			{
				if (value != _placeholderSpan.Text)
				{
					_placeholderSpan.Text = value;
					ApplyPlaceholderAndStyle();
				}
			}
		}

		/// <summary>
		/// Gets or sets the color of the placeholder's text.
		/// </summary>
		/// <value>The color of the placeholder's text.</value>
		public EColor PlaceholderColor
		{
			get
			{
				return _placeholderSpan.ForegroundColor;
			}

			set
			{
				if (!_placeholderSpan.ForegroundColor.Equals(value))
				{
					_placeholderSpan.ForegroundColor = value;
					ApplyPlaceholderAndStyle();
				}
			}
		}

		/// <summary>
		/// Implementation of the IMeasurable.Measure() method.
		/// </summary>
		public virtual ESize Measure(int availableWidth, int availableHeight)
		{
			var originalSize = Geometry;
			// resize the control using the whole available width
			Resize(availableWidth, originalSize.Height);

			ESize rawSize;
			ESize formattedSize;
			var edjeTextBlock = EdjeObject["elm.guide"];

			// if there's no text, but there's a placeholder, use it for measurements
			if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder) && edjeTextBlock != null)
			{
				rawSize = edjeTextBlock.TextBlockNativeSize;
				formattedSize = edjeTextBlock.TextBlockFormattedSize;
			}
			else
			{
				// there's text in the entry, use it instead
				rawSize = Native.TextHelper.GetRawTextBlockSize(this);
				formattedSize = Native.TextHelper.GetFormattedTextBlockSize(this);
			}

			// restore the original size
			Resize(originalSize.Width, originalSize.Height);

			// Set bottom padding for lower case letters that have segments below the bottom line of text (g, j, p, q, y).
			var verticalPadding = (int)Math.Ceiling(0.05 * FontSize);
			var horizontalPadding = (int)Math.Ceiling(0.2 * FontSize);
			rawSize.Height += verticalPadding;
			formattedSize.Height += verticalPadding;
			formattedSize.Width += horizontalPadding;

			// if the raw text width is larger than available width, we use the available width,
			// while height is set to the smallest height value
			if (rawSize.Width > availableWidth)
			{
				return new ESize
				{
					Width = availableWidth,
					Height = Math.Min(formattedSize.Height, Math.Max(rawSize.Height, availableHeight)),
				};
			}
			else
			{
				// width is fine, return the formatted text size
				return formattedSize;
			}
		}

		protected virtual void OnTextChanged(string oldValue, string newValue)
		{
			TextChanged?.Invoke(this, new TextChangedEventArgs(oldValue, newValue));
		}

		void IBatchable.OnBatchCommitted()
		{
			ApplyTextAndStyle();
		}

		/// <summary>
		/// Applies entry's text and its style.
		/// </summary>
		void ApplyTextAndStyle()
		{
			if (!this.IsBatched())
			{
				SetInternalTextAndStyle(_span.GetDecoratedText(), _span.GetStyle());
			}
		}

		/// <summary>
		/// Sets entry's internal text and its style.
		/// </summary>
		/// <param name="formattedText">Formatted text, supports HTML tags.</param>
		/// <param name="textStyle">Style applied to the formattedText.</param>
		void SetInternalTextAndStyle(string formattedText, string textStyle)
		{
			if (_changedByUserCallbackDepth == 0)
			{
				base.Text = formattedText;
				base.TextStyle = textStyle;
			}
		}

		/// <summary>
		/// Gets the internal text representation of the entry.
		/// </summary>
		/// <returns>The internal text representation.</returns>
		string GetInternalText()
		{
			return Entry.ConvertMarkupToUtf8(base.Text);
		}

		/// <summary>
		/// Applies the keyboard type to be used by the entry.
		/// </summary>
		/// <param name="keyboard">Keyboard type to be used.</param>
		void ApplyKeyboard(Keyboard keyboard)
		{
			SetInternalKeyboard(_keyboard = keyboard);
		}

		/// <summary>
		/// Configures the ElmSharp.Entry with specified keyboard type and displays
		/// the keyboard automatically unless the provided type is Keyboard.None.
		/// </summary>
		/// <param name="keyboard">Keyboard type to be used.</param>
		void SetInternalKeyboard(Keyboard keyboard)
		{
			if (keyboard == Keyboard.None)
			{
				SetInputPanelEnabled(false);
			}
			else if (Keyboard == Keyboard.Numeric)
			{
				SetInputPanelEnabled(true);
				SetInputPanelLayout(InputPanelLayout.NumberOnly);
				// InputPanelVariation is used to allow using deciaml point.
				InputPanelVariation = s_VariationSignedAndDecimal;
			}
			else
			{
				SetInputPanelEnabled(true);
				SetInputPanelLayout((InputPanelLayout)keyboard);
				InputPanelVariation = s_VariationNormal;
			}
		}

		/// <summary>
		/// Applies placeholders's text and its style.
		/// </summary>
		void ApplyPlaceholderAndStyle()
		{
			SetInternalPlaceholderAndStyle(_placeholderSpan.GetMarkupText());
		}

		/// <summary>
		/// Sets placeholder's internal text and style.
		/// </summary>
		/// <param name="markupText">Markup text to be used as a placeholder.</param>
		void SetInternalPlaceholderAndStyle(string markupText)
		{
			SetPartText("elm.guide", markupText ?? "");
		}
	}
}
