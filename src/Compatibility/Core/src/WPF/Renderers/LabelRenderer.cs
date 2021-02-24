using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Maui.Controls.Compatibility.Platform.WPF.Extensions;
using WThickness = System.Windows.Thickness;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	public class LabelRenderer : ViewRenderer<Label, TextBlock>
	{
		bool _fontApplied;
		IList<double> _inlineHeights = new List<double>();

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new TextBlock());
				}

				// Update control property 
				UpdateText();
				UpdateTextDecorations();
				UpdateColor();
				UpdateHorizontalTextAlign();
				UpdateVerticalTextAlign();
				UpdateFont();
				UpdateLineBreakMode();
				UpdatePadding();
			}

			base.OnElementChanged(e);
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = base.GetDesiredSize(widthConstraint, heightConstraint);
			Control.RecalculateSpanPositions(Element, _inlineHeights);
			return size;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.TextProperty.PropertyName ||
				e.PropertyName == Label.FormattedTextProperty.PropertyName ||
				e.PropertyName == Label.TextTransformProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.TextDecorationsProperty.PropertyName)
				UpdateTextDecorations();
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlign();
			else if (e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateVerticalTextAlign();
			else if (e.PropertyName == Label.FontAttributesProperty.PropertyName ||
				e.PropertyName == Label.FontFamilyProperty.PropertyName ||
				e.PropertyName == Label.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
			else if (e.PropertyName == Label.PaddingProperty.PropertyName)
				UpdatePadding();
		}

		protected override void UpdateBackground()
		{
			Brush background = Element.Background;

			if (Brush.IsNullOrEmpty(background))
				Control.UpdateDependencyColor(TextBlock.BackgroundProperty, Element.BackgroundColor);
			else
				Control.Background = background.ToBrush();
		}

		void UpdateTextDecorations()
		{
			if (!Element.IsSet(Label.TextDecorationsProperty))
				return;

			var textDecorations = Element.TextDecorations;

			var newTextDecorations = new System.Windows.TextDecorationCollection(Control.TextDecorations);

			if ((textDecorations & TextDecorations.Underline) == 0)
				newTextDecorations.TryRemove(System.Windows.TextDecorations.Underline, out newTextDecorations);
			else
				newTextDecorations.Add(System.Windows.TextDecorations.Underline);

			if ((textDecorations & TextDecorations.Strikethrough) == 0)
				newTextDecorations.TryRemove(System.Windows.TextDecorations.Strikethrough, out newTextDecorations);
			else
				newTextDecorations.Add(System.Windows.TextDecorations.Strikethrough);

			Control.TextDecorations = newTextDecorations;
		}


		void UpdateHorizontalTextAlign()
		{
			if (Control == null)
				return;

			Label label = Element;
			if (label == null)
				return;

			Control.TextAlignment = label.HorizontalTextAlignment.ToNativeTextAlignment();
		}

		void UpdateVerticalTextAlign()
		{
			if (Control == null)
				return;

			Label label = Element;
			if (label == null)
				return;

			Control.VerticalAlignment = label.VerticalTextAlignment.ToNativeVerticalAlignment();
		}

		void UpdateColor()
		{
			if (Control == null || Element == null)
				return;

			if (Element.TextColor != Color.Default)
				Control.Foreground = Element.TextColor.ToBrush();
			else
				Control.Foreground = Brushes.Black;
		}

		void UpdateFont()
		{
			if (Control == null)
				return;

			Label label = Element;
			if (label == null || (label.IsDefault() && !_fontApplied))
				return;

#pragma warning disable 618
			Font fontToApply = label.IsDefault() ? Font.SystemFontOfSize(NamedSize.Medium) : label.Font;
#pragma warning restore 618

			Control.ApplyFont(fontToApply);
			_fontApplied = true;
		}

		void UpdateLineBreakMode()
		{
			if (Control == null)
				return;

			switch (Element.LineBreakMode)
			{
				case LineBreakMode.NoWrap:
					Control.TextTrimming = TextTrimming.None;
					Control.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.WordWrap:
					Control.TextTrimming = TextTrimming.None;
					Control.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.CharacterWrap:
					Control.TextTrimming = TextTrimming.CharacterEllipsis;
					Control.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.HeadTruncation:
					Control.TextTrimming = TextTrimming.WordEllipsis;
					Control.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.TailTruncation:
					Control.TextTrimming = TextTrimming.CharacterEllipsis;
					Control.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.MiddleTruncation:
					Control.TextTrimming = TextTrimming.WordEllipsis;
					Control.TextWrapping = TextWrapping.NoWrap;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void UpdateText()
		{
			if (Control == null)
				return;

			Label label = Element;
			if (label != null)
			{
				if (label.FormattedText == null)
					Control.Text = label.UpdateFormsText(label.Text, label.TextTransform);
				else
				{
					FormattedString formattedText = label.FormattedText ?? label.Text;

					Control.Inlines.Clear();
					// Have to implement a measure here, otherwise inline.ContentStart and ContentEnd will be null, when used in RecalculatePositions
					Control.Measure(new System.Windows.Size(double.MaxValue, double.MaxValue));

					var heights = new List<double>();
					for (var i = 0; i < formattedText.Spans.Count; i++)
					{
						var span = formattedText.Spans[i];
						var run = span.ToRun();
						heights.Add(Control.FindDefaultLineHeight(run));
						Control.Inlines.Add(run);
					}
					_inlineHeights = heights;
				}
			}
		}

		void UpdatePadding()
		{
			if (Control == null || Element == null)
			{
				return;
			}

			Control.Padding = new WThickness(
					Element.Padding.Left,
					Element.Padding.Top,
					Element.Padding.Right,
					Element.Padding.Bottom);
		}
	}
}
