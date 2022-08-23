using System;
using ElmSharp;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using TColor = Tizen.UIExtensions.Common.Color;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Platform
{
	public class MauiRadioButton : Radio, IMeasurable, IBatchable
	{
		public MauiRadioButton(EvasObject parent) : base(parent)
		{
		}

		readonly Span _span = new Span();

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

		public TColor TextColor
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

		public virtual TSize Measure(double availableWidth, double availableHeight)
		{
			Resize(availableWidth.ToScaledPixel(), Geometry.Height);
			var formattedSize = this.GetTextBlockFormattedSize();
			Resize(Geometry.Width, Geometry.Height);
			return new TSize
			{
				Width = MinimumWidth + formattedSize.Width,
				Height = Math.Max(MinimumHeight, formattedSize.Height)
			};
		}

		void IBatchable.OnBatchCommitted()
		{
			ApplyTextAndStyle();
		}

		void ApplyTextAndStyle()
		{
			if (!this.IsBatched())
			{
				SetInternalTextAndStyle(_span.GetDecoratedText(), _span.GetStyle());
			}
		}

		void SetInternalTextAndStyle(string formattedText, string textStyle)
		{
			bool isVisible = true;
			if (string.IsNullOrEmpty(formattedText))
			{
				base.Text = null;
				this.SetTextBlockStyle("");
				this.SendTextVisibleSignal(false);
			}
			else
			{
				base.Text = formattedText;
				this.SetTextBlockStyle(textStyle);
				this.SendTextVisibleSignal(isVisible);
			}
		}
	}
}