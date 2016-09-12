using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public static class FormattedStringExtensions
	{
		public static Run ToRun(this Span span)
		{
			var run = new Run { Text = span.Text };

			if (span.ForegroundColor != Color.Default)
				run.Foreground = span.ForegroundColor.ToBrush();

			if (!span.IsDefault())
#pragma warning disable 618
				run.ApplyFont(span.Font);
#pragma warning restore 618

			return run;
		}
	}

	public class LabelRenderer : ViewRenderer<Label, TextBlock>
	{
		bool _fontApplied;
		bool _isInitiallyDefault;

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (Element == null)
				return finalSize;
			double childHeight = Math.Max(0, Math.Min(Element.Height, Control.DesiredSize.Height));
			var rect = new Rect();

			switch (Element.VerticalTextAlignment)
			{
				case TextAlignment.Start:
					break;
				default:
				case TextAlignment.Center:
					rect.Y = (int)((finalSize.Height - childHeight) / 2);
					break;
				case TextAlignment.End:
					rect.Y = finalSize.Height - childHeight;
					break;
			}
			rect.Height = childHeight;
			rect.Width = finalSize.Width;
			Control.Arrange(rect);
			return finalSize;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new TextBlock());
				}

				_isInitiallyDefault = Element.IsDefault();

				UpdateText(Control);
				UpdateColor(Control);
				UpdateAlign(Control);
				UpdateFont(Control);
				UpdateLineBreakMode(Control);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Label.TextProperty.PropertyName || e.PropertyName == Label.FormattedTextProperty.PropertyName)
				UpdateText(Control);
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateColor(Control);
			else if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateAlign(Control);
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateFont(Control);
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode(Control);

			base.OnElementPropertyChanged(sender, e);
		}

		void UpdateAlign(TextBlock textBlock)
		{
			if (textBlock == null)
				return;

			Label label = Element;
			if (label == null)
				return;

			textBlock.TextAlignment = label.HorizontalTextAlignment.ToNativeTextAlignment();
			textBlock.VerticalAlignment = label.VerticalTextAlignment.ToNativeVerticalAlignment();
		}

		void UpdateColor(TextBlock textBlock)
		{
			if (textBlock == null)
				return;

			Label label = Element;
			if (label != null && label.TextColor != Color.Default)
			{
				textBlock.Foreground = label.TextColor.ToBrush();
			}
			else
			{
				textBlock.ClearValue(TextBlock.ForegroundProperty);
			}
		}

		void UpdateFont(TextBlock textBlock)
		{
			if (textBlock == null)
				return;

			Label label = Element;
			if (label == null || (label.IsDefault() && !_fontApplied))
				return;

#pragma warning disable 618
			Font fontToApply = label.IsDefault() && _isInitiallyDefault ? Font.SystemFontOfSize(NamedSize.Medium) : label.Font;
#pragma warning restore 618

			textBlock.ApplyFont(fontToApply);
			_fontApplied = true;
		}

		void UpdateLineBreakMode(TextBlock textBlock)
		{
			if (textBlock == null)
				return;

			switch (Element.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					textBlock.TextTrimming = TextTrimming.Clip;
					textBlock.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.WordWrap:
					textBlock.TextTrimming = TextTrimming.None;
					textBlock.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.CharacterWrap:
					textBlock.TextTrimming = TextTrimming.WordEllipsis;
					textBlock.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.HeadTruncation:
					textBlock.TextTrimming = TextTrimming.WordEllipsis;
					textBlock.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.TailTruncation:
					textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
					textBlock.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.MiddleTruncation:
					textBlock.TextTrimming = TextTrimming.WordEllipsis;
					textBlock.TextWrapping = TextWrapping.NoWrap;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void UpdateText(TextBlock textBlock)
		{
			if (textBlock == null)
				return;

			Label label = Element;
			if (label != null)
			{
				FormattedString formatted = label.FormattedText;

				if (formatted == null)
				{
					textBlock.Text = label.Text ?? string.Empty;
				}
				else
				{
					textBlock.Inlines.Clear();

					for (var i = 0; i < formatted.Spans.Count; i++)
					{
						textBlock.Inlines.Add(formatted.Spans[i].ToRun());
					}
				}
			}
		}
	}
}