using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Windows.UI.Text;
using Microsoft.Maui.Controls.Compatibility.Platform.UAP;
using Microsoft.Maui.Controls.Compatibility.Platform.UAP.Extensions;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Label;
using WRect = Windows.Foundation.Rect;
using WThickness = Microsoft.UI.Xaml.Thickness;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class FormattedStringExtensions
	{
		public static Run ToRun(this Span span)
		{
			var run = new Run { Text = span.Text ?? string.Empty };

			if (span.TextColor != Color.Default)
				run.Foreground = span.TextColor.ToBrush();

			if (!span.IsDefault())
				run.ApplyFont(span);

			if (span.IsSet(Span.TextDecorationsProperty))
				run.TextDecorations = (Windows.UI.Text.TextDecorations)span.TextDecorations;

			run.CharacterSpacing = span.CharacterSpacing.ToEm();

			return run;
		}
	}

	public class LabelRenderer : ViewRenderer<Label, TextBlock>
	{
		bool _fontApplied;
		bool _isInitiallyDefault;
		SizeRequest _perfectSize;
		bool _perfectSizeValid;
		IList<double> _inlineHeights = new List<double>();

		//TODO: We need to revisit this later when we complete the UI Tests for UWP.
		// Changing the AutomationPeer here prevents the Narrator from functioning properly.
		// Oddly, it affects more than just the TextBlocks. It seems to break the entire scan mode.

		//protected override AutomationPeer OnCreateAutomationPeer()
		//{
		//	// We need an automation peer so we can interact with this in automated tests
		//	if (Control == null)
		//	{
		//		return new FrameworkElementAutomationPeer(this);
		//	}

		//	return new TextBlockAutomationPeer(Control);
		//}

		public LabelRenderer()
		{
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (Element == null)
				return finalSize;

			double childHeight = Math.Max(0, Math.Min(Element.Height, Control.DesiredSize.Height));
			var rect = new WRect();

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
			Control.RecalculateSpanPositions(Element, _inlineHeights);
			return finalSize;
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (!_perfectSizeValid)
			{
				_perfectSize = base.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
				_perfectSize.Minimum = new Size(Math.Min(10, _perfectSize.Request.Width), _perfectSize.Request.Height);
				_perfectSizeValid = true;
			}

			var widthFits = widthConstraint >= _perfectSize.Request.Width;
			var heightFits = heightConstraint >= _perfectSize.Request.Height;

			if (widthFits && heightFits)
				return _perfectSize;

			var result = base.GetDesiredSize(widthConstraint, heightConstraint);
			var tinyWidth = Math.Min(10, result.Request.Width);
			result.Minimum = new Size(tinyWidth, result.Request.Height);

			if (widthFits || Element.LineBreakMode == LineBreakMode.NoWrap)
				return result;

			bool containerIsNotInfinitelyWide = !double.IsInfinity(widthConstraint);

			if (containerIsNotInfinitelyWide)
			{
				bool textCouldHaveWrapped = Element.LineBreakMode == LineBreakMode.WordWrap || Element.LineBreakMode == LineBreakMode.CharacterWrap;
				bool textExceedsContainer = result.Request.Width > widthConstraint;

				if (textExceedsContainer || textCouldHaveWrapped)
				{
					var expandedWidth = Math.Max(tinyWidth, widthConstraint);
					result.Request = new Size(expandedWidth, result.Request.Height);
				}
			}

			return result;
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
				UpdateTextDecorations(Control);
				UpdateColor(Control);
				UpdateAlign(Control);
				UpdateCharacterSpacing(Control);
				UpdateFont(Control);
				UpdateLineBreakMode(Control);
				UpdateMaxLines(Control);
				UpdateDetectReadingOrderFromContent(Control);
				UpdateLineHeight(Control);
				UpdatePadding(Control);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(Label.TextProperty, Label.FormattedTextProperty, Label.TextTransformProperty, Label.TextTypeProperty))
				UpdateText(Control);
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateColor(Control);
			else if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateAlign(Control);
			else if (e.PropertyName == Label.FontAttributesProperty.PropertyName || e.PropertyName == Label.FontFamilyProperty.PropertyName || e.PropertyName == Label.FontSizeProperty.PropertyName)
				UpdateFont(Control);
			else if (e.PropertyName == Label.TextDecorationsProperty.PropertyName)
				UpdateTextDecorations(Control);
			else if (e.PropertyName == Label.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing(Control);
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode(Control);
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateAlign(Control);
			else if (e.PropertyName == Specifics.DetectReadingOrderFromContentProperty.PropertyName)
				UpdateDetectReadingOrderFromContent(Control);
			else if (e.PropertyName == Label.LineHeightProperty.PropertyName)
				UpdateLineHeight(Control);
			else if (e.PropertyName == Label.MaxLinesProperty.PropertyName)
				UpdateMaxLines(Control);
			else if (e.PropertyName == Label.PaddingProperty.PropertyName)
				UpdatePadding(Control);

			base.OnElementPropertyChanged(sender, e);
		}

		void UpdateTextDecorations(TextBlock textBlock)
		{
			if (!Element.IsSet(Label.TextDecorationsProperty))
				return;

			var elementTextDecorations = Element.TextDecorations;

			if ((elementTextDecorations & TextDecorations.Underline) == 0)
				textBlock.TextDecorations &= ~Windows.UI.Text.TextDecorations.Underline;
			else
				textBlock.TextDecorations |= Windows.UI.Text.TextDecorations.Underline;

			if ((elementTextDecorations & TextDecorations.Strikethrough) == 0)
				textBlock.TextDecorations &= ~Windows.UI.Text.TextDecorations.Strikethrough;
			else
				textBlock.TextDecorations |= Windows.UI.Text.TextDecorations.Strikethrough;

			//TextDecorations are not updated in the UI until the text changes
			if (textBlock.Inlines != null && textBlock.Inlines.Count > 0)
			{
				for (var i = 0; i < textBlock.Inlines.Count; i++)
				{
					var run = (Run)textBlock.Inlines[i];
					run.Text = run.Text;
				}
			}
			else
			{
				textBlock.Text = textBlock.Text;
			}

		}

		void UpdateAlign(TextBlock textBlock)
		{
			_perfectSizeValid = false;

			if (textBlock == null)
				return;

			Label label = Element;
			if (label == null)
				return;

			textBlock.TextAlignment = label.HorizontalTextAlignment.ToNativeTextAlignment(((IVisualElementController)Element).EffectiveFlowDirection);
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
			_perfectSizeValid = false;

			if (textBlock == null)
				return;

			Label label = Element;
			if (label == null || (label.IsDefault() && !_fontApplied))
				return;

			if (label.IsDefault() && _isInitiallyDefault)
				textBlock.ApplyFont(Font.SystemFontOfSize(NamedSize.Medium));
			else
				textBlock.ApplyFont(label);

			_fontApplied = true;
		}

		void UpdateLineBreakMode(TextBlock textBlock)
		{
			_perfectSizeValid = false;

			if (textBlock == null)
				return;

			textBlock.UpdateLineBreakMode(Element.LineBreakMode);
		}

		void UpdateCharacterSpacing(TextBlock textBlock)
		{
			textBlock.CharacterSpacing = Element.CharacterSpacing.ToEm();
		}


		void DetermineTruncatedTextWrapping(TextBlock textBlock)
		{
			if (Element.MaxLines > 1)
				textBlock.TextWrapping = TextWrapping.Wrap;
			else
				textBlock.TextWrapping = TextWrapping.NoWrap;
		}

		void UpdateText(TextBlock textBlock)
		{
			_perfectSizeValid = false;

			if (textBlock == null)
				return;

			switch (Element.TextType)
			{
				case TextType.Html:
					UpdateTextHtml(textBlock);
					break;

				default:
					UpdateTextPlainText(textBlock);
					break;
			}
		}

		void UpdateTextPlainText(TextBlock textBlock)
		{
			Label label = Element;
			if (label != null)
			{
				FormattedString formatted = label.FormattedText;

				if (formatted == null)
				{
					textBlock.Text = label.UpdateFormsText(label.Text, label.TextTransform);
				}
				else
				{
					textBlock.Inlines.Clear();
					// Have to implement a measure here, otherwise inline.ContentStart and ContentEnd will be null, when used in RecalculatePositions
					textBlock.Measure(new Windows.Foundation.Size(double.MaxValue, double.MaxValue));

					var heights = new List<double>();
					for (var i = 0; i < formatted.Spans.Count; i++)
					{
						var span = formatted.Spans[i];

						var run = span.ToRun();
						heights.Add(Control.FindDefaultLineHeight(run));
						textBlock.Inlines.Add(run);
					}
					_inlineHeights = heights;
				}
			}
		}

		void UpdateTextHtml(TextBlock textBlock)
		{
			var text = Element.Text ?? String.Empty;

			// Just in case we are not given text with elements.
			var modifiedText = string.Format("<div>{0}</div>", text);
			modifiedText = Regex.Replace(modifiedText, "<br>", "<br></br>", RegexOptions.IgnoreCase);
			// reset the text because we will add to it.
			Control.Inlines.Clear();
			try
			{
				var element = XElement.Parse(modifiedText);
				LabelHtmlHelper.ParseText(element, Control.Inlines, Element);
			}
			catch (Exception)
			{
				// if anything goes wrong just show the html
				textBlock.Text = Windows.Data.Html.HtmlUtilities.ConvertToText(Element.Text);
			}
		}

		void UpdateDetectReadingOrderFromContent(TextBlock textBlock)
		{
			if (Element.IsSet(Specifics.DetectReadingOrderFromContentProperty))
			{
				if (Element.OnThisPlatform().GetDetectReadingOrderFromContent())
				{
					textBlock.TextReadingOrder = TextReadingOrder.DetectFromContent;
				}
				else
				{
					textBlock.TextReadingOrder = TextReadingOrder.UseFlowDirection;
				}
			}
		}

		void UpdateLineHeight(TextBlock textBlock)
		{
			if (textBlock == null)
				return;

			if (Element.LineHeight >= 0)
			{
				textBlock.LineHeight = Element.LineHeight * textBlock.FontSize;
			}
		}

		void UpdateMaxLines(TextBlock textBlock)
		{
			if (Element.MaxLines >= 0)
			{
				textBlock.MaxLines = Element.MaxLines;
			}
			else
			{
				textBlock.MaxLines = 0;
			}
		}

		void UpdatePadding(TextBlock textBlock)
		{
			textBlock.Padding = new WThickness(
					Element.Padding.Left,
					Element.Padding.Top,
					Element.Padding.Right,
					Element.Padding.Bottom);
		}
	}
}