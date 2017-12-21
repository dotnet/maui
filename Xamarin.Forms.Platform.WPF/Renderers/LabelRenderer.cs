using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF
{
	public class LabelRenderer : ViewRenderer<Label, TextBlock>
	{
		bool _fontApplied;
		
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
				UpdateColor();
				UpdateAlign();
				UpdateFont();
				UpdateLineBreakMode();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Label.TextProperty.PropertyName || e.PropertyName == Label.FormattedTextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Label.TextColorProperty.PropertyName)
				UpdateColor();
			else if (e.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Label.VerticalTextAlignmentProperty.PropertyName)
				UpdateAlign();
			else if (e.PropertyName == Label.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Label.LineBreakModeProperty.PropertyName)
				UpdateLineBreakMode();
		}

		protected override void UpdateBackground()
		{
			Control.UpdateDependencyColor(TextBlock.BackgroundProperty, Element.BackgroundColor);
		}

		void UpdateAlign()
		{
			if (Control == null)
				return;

			Label label = Element;
			if (label == null)
				return;

			Control.TextAlignment = label.HorizontalTextAlignment.ToNativeTextAlignment();
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
					Control.Text = label.Text;
				else
				{
					FormattedString formattedText = label.FormattedText ?? label.Text;

					Control.Inlines.Clear();
					foreach (Inline inline in formattedText.ToInlines())
						Control.Inlines.Add(inline);
				}
			}
		}
		
	}
}
