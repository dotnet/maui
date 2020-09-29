using System;
using ElmSharp;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using ESize = ElmSharp.Size;
using TSButtonStyle = Xamarin.Forms.PlatformConfiguration.TizenSpecific.ButtonStyle;

#if __MATERIAL__
using Tizen.NET.MaterialComponents;
#endif

namespace Xamarin.Forms.Platform.Tizen.Native
{
#if __MATERIAL__
	public class MaterialButton : MButton, IMeasurable, IBatchable, IButton
	{
		public MaterialButton(EvasObject parent) : base(parent)
		{
		}
#else
	/// <summary>
	/// Extends the EButton control, providing basic formatting features,
	/// i.e. font color, size, additional image.
	/// </summary>
	public class Button : EButton, IMeasurable, IBatchable, IButton
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.Button"/> class.
		/// </summary>
		/// <param name="parent">Parent evas object.</param>
		public Button(EvasObject parent) : base(parent)
		{
		}
#endif
		/// <summary>
		/// Holds the formatted text of the button.
		/// </summary>
		readonly Span _span = new Span();

		/// <summary>
		/// Optional image, if set will be drawn on the button.
		/// </summary>
		Image _image;

		/// <summary>
		/// Gets or sets the button's text.
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
					_span.Text = value;
					ApplyTextAndStyle();
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
		/// Gets or sets the color of the text background.
		/// </summary>
		/// <value>The color of the text background.</value>
		public EColor TextBackgroundColor
		{
			get
			{
				return _span.BackgroundColor;
			}

			set
			{
				if (!_span.BackgroundColor.Equals(value))
				{
					_span.BackgroundColor = value;
					ApplyTextAndStyle();
				}
			}
		}

		/// <summary>
		/// Gets or sets the font family.
		/// </summary>
		/// <value>The font family.</value>
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
				}
			}
		}

		/// <summary>
		/// Gets or sets the font attributes.
		/// </summary>
		/// <value>The font attributes.</value>
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
				}
			}
		}

		/// <summary>
		/// Gets or sets the size of the font.
		/// </summary>
		/// <value>The size of the font.</value>
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
				}
			}
		}

		/// <summary>
		/// Gets or sets the image to be displayed next to the button's text.
		/// </summary>
		/// <value>The image displayed on the button.</value>
		public Image Image
		{
			get
			{
				return _image;
			}

			set
			{
				if (value != _image)
				{
					ApplyImage(value);
				}
			}
		}

		/// <summary>
		/// Implementation of the IMeasurable.Measure() method.
		/// </summary>
		public virtual ESize Measure(int availableWidth, int availableHeight)
		{
			if (Style == TSButtonStyle.Circle)
			{
				return new ESize(MinimumWidth, MinimumHeight);
			}
			else
			{
				if (Image != null)
					MinimumWidth += Image.Geometry.Width;

				var rawSize = this.GetTextBlockNativeSize();
				return new ESize(rawSize.Width + MinimumWidth, Math.Max(MinimumHeight, rawSize.Height));
			}
		}

		void IBatchable.OnBatchCommitted()
		{
			ApplyTextAndStyle();
		}

		/// <summary>
		/// Applies the button's text and its style.
		/// </summary>
		void ApplyTextAndStyle()
		{
			if (!this.IsBatched())
			{
				SetInternalTextAndStyle(_span.GetDecoratedText(), _span.GetStyle());
			}
		}

		/// <summary>
		/// Sets the button's internal text and its style.
		/// </summary>
		/// <param name="formattedText">Formatted text, supports HTML tags.</param>
		/// <param name="textStyle">Style applied to the formattedText.</param>
		void SetInternalTextAndStyle(string formattedText, string textStyle)
		{
			bool isVisible = true;
			if (string.IsNullOrEmpty(formattedText))
			{
				formattedText = null;
				textStyle = null;
				isVisible = false;
			}
			base.Text = formattedText;
			this.SetTextBlockStyle(textStyle);
			this.SendTextVisibleSignal(isVisible);
		}

		/// <summary>
		/// Applies the image to be displayed on the button. If value is <c>null</c>,
		/// image will be removed.
		/// </summary>
		/// <param name="image">Image to be displayed or null.</param>
		void ApplyImage(Image image)
		{
			_image = image;

			SetInternalImage();
		}

		/// <summary>
		/// Sets the internal image. If value is <c>null</c>, image will be removed.
		/// </summary>
		void SetInternalImage()
		{
			this.SetIconPart(_image);
		}

		/// <summary>
		/// Update the button's style
		/// </summary>
		/// <param name="style">The style of button</param>
		public void UpdateStyle(string style)
		{
			if (Style != style)
			{
				Style = style;
				if (Style == TSButtonStyle.Default)
					_span.HorizontalTextAlignment = TextAlignment.Auto;
				else
					_span.HorizontalTextAlignment = TextAlignment.Center;
				ApplyTextAndStyle();
			}
		}
	}
}