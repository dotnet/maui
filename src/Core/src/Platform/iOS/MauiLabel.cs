#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Platform
{
	public class MauiLabel : UILabel, IUIViewLifeCycleEvents
	{
		UIControlContentVerticalAlignment _verticalAlignment = UIControlContentVerticalAlignment.Center;
		// Owned by this label's own view hierarchy and released when selection is turned off, so it cannot outlive the label.
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The text view is a subview of this label and does not reference it.")]
		UITextView _selectionTextView;
		// The cache keeps its inputs alive, so a native handle can't be reused by a different object while it is compared.
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Plain value objects that do not reference this label.")]
		NSAttributedString _selectionTextCache;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Plain value objects that do not reference this label.")]
		NSAttributedString _selectionTextSource;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Plain value objects that do not reference this label.")]
		UIFont _selectionTextFont;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Plain value objects that do not reference this label.")]
		UIColor _selectionTextColor;
		// Stands in for a null AttributedText so the cache can match it by handle instead of rebuilding on every layout pass.
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Plain value object that does not reference this label.")]
		NSAttributedString _emptySelectionSource;
		nint _selectionTextLines;

		UIEdgeInsets _textInsets;

		public UIEdgeInsets TextInsets
		{
			get => _textInsets;
			set
			{
				_textInsets = value;
				InvalidateSelectionLayout();
			}
		}

		internal UIControlContentVerticalAlignment VerticalAlignment
		{
			get => _verticalAlignment;
			set
			{
				_verticalAlignment = value;
				InvalidateSelectionLayout();
				SetNeedsDisplay();
			}
		}

		public MauiLabel(RectangleF frame) : base(frame)
		{
		}

		public MauiLabel()
		{
		}

		// UILabel has no native text selection. While selectable, a UITextView hosted inside this label renders
		// and selects the text, and the label keeps measuring it so layout is unchanged.
		internal bool IsTextSelectable
		{
			get => _selectionTextView is not null;
			set
			{
				if (value == IsTextSelectable)
				{
					return;
				}

				if (value)
				{
					_selectionTextView = CreateSelectionTextView();
					AddSubview(_selectionTextView);
					SyncSelectionTextView();
				}
				else
				{
					_selectionTextView.RemoveFromSuperview();
					_selectionTextView.Dispose();
					_selectionTextView = null;
					_selectionTextCache = null;
					_selectionTextSource = null;
					_selectionTextFont = null;
					_selectionTextColor = null;
					_emptySelectionSource = null;
				}

				SetNeedsDisplay();
			}
		}

		internal bool IsSelectionView(UIView view) =>
			_selectionTextView is not null && view is not null && view.IsDescendantOfView(_selectionTextView);

		// Only text glyphs start a selection. Touches on the empty part of the text view fall through to the label,
		// so the label's own gesture recognizers behave the same as when it is not selectable.
		public override UIView HitTest(CoreGraphics.CGPoint point, UIEvent uievent)
		{
			var hit = base.HitTest(point, uievent);

			// While text is selected, the handles and the area around the text must keep reaching the text view.
			if (_selectionTextView is { } textView && hit is not null && hit.IsDescendantOfView(textView) &&
				textView.SelectedRange.Length == 0 &&
				!IsPointOnText(textView, ConvertPointToView(point, textView)))
			{
				return this;
			}

			return hit;
		}

		static bool IsPointOnText(UITextView textView, CoreGraphics.CGPoint point)
		{
			if (textView.GetClosestPositionToPoint(point) is not { } position)
			{
				return false;
			}

			var start = textView.BeginningOfDocument;
			var index = (int)textView.GetOffsetFromPosition(start, position);
			var length = textView.AttributedText?.Length ?? 0;

			for (var i = Math.Max(index - 1, 0); i <= index && i < length; i++)
			{
				var range = textView.GetTextRange(textView.GetPosition(start, i), textView.GetPosition(start, i + 1));

				// Allow a little vertical slack so the gaps between lines still count as text.
				if (range is not null && RectangleF.Inflate(textView.GetFirstRectForRange(range), 0, 4).Contains(point))
				{
					return true;
				}
			}

			return false;
		}

		static UITextView CreateSelectionTextView()
		{
			var textView = new UITextView(RectangleF.Empty)
			{
				Editable = false,
				Selectable = true,
				ScrollEnabled = false,
				Opaque = false,
				BackgroundColor = UIColor.Clear,
				TextContainerInset = UIEdgeInsets.Zero,
				DataDetectorTypes = UIDataDetectorType.None,
				IsAccessibilityElement = false,
			};

			textView.TextContainer.LineFragmentPadding = 0;

			return textView;
		}

		// UILabel does not redraw, and so does not reach DrawText, when its text becomes empty.
		public override string Text
		{
			get => base.Text;
			set
			{
				base.Text = value;
				InvalidateSelectionLayout();
			}
		}

		public override NSAttributedString AttributedText
		{
			get => base.AttributedText;
			set
			{
				base.AttributedText = value;
				InvalidateSelectionLayout();
			}
		}

		void InvalidateSelectionLayout()
		{
			if (_selectionTextView is not null)
			{
				SetNeedsLayout();
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			SyncSelectionTextView();
		}

		// UILabel falls back to its own font and color for text without those attributes, a text view does not.
		// A truncating paragraph style also makes a text view cut every paragraph to one line, whereas UILabel only
		// truncates the last visible line. The text container applies the truncation for multi-line labels instead.
		NSAttributedString GetSelectionText()
		{
			var source = AttributedText ?? (_emptySelectionSource ??= new NSAttributedString());
			var font = Font;
			var textColor = TextColor;

			// This runs for every redraw and layout pass, so only rebuild when one of its inputs changed.
			if (_selectionTextCache is not null &&
				_selectionTextSource?.Handle == source.Handle &&
				_selectionTextFont?.Handle == font?.Handle &&
				_selectionTextColor?.Handle == textColor?.Handle &&
				_selectionTextLines == Lines)
			{
				return _selectionTextCache;
			}

			var result = BuildSelectionText(source);

			_selectionTextCache = result;
			_selectionTextSource = source;
			_selectionTextFont = font;
			_selectionTextColor = textColor;
			_selectionTextLines = Lines;

			return result;
		}

		NSAttributedString BuildSelectionText(NSAttributedString source)
		{

			if (source.Length == 0)
			{
				return source;
			}

			var result = new NSMutableAttributedString(source);
			var fullRange = new NSRange(0, source.Length);

			// UILabel never opens links, so do not let the text view start doing it for HTML text.
			result.RemoveAttribute(UIStringAttributeKey.Link, fullRange);

			FillMissingAttribute(source, result, UIStringAttributeKey.Font, Font);
			FillMissingAttribute(source, result, UIStringAttributeKey.ForegroundColor, TextColor);

			if (Lines != 1)
			{
				source.EnumerateAttribute(UIStringAttributeKey.ParagraphStyle, fullRange, NSAttributedStringEnumeration.None, (NSObject value, NSRange range, ref bool stop) =>
				{
					if (value is NSParagraphStyle style &&
						style.LineBreakMode is UILineBreakMode.TailTruncation or UILineBreakMode.HeadTruncation or UILineBreakMode.MiddleTruncation)
					{
						var wrapping = (NSMutableParagraphStyle)style.MutableCopy();
						wrapping.LineBreakMode = UILineBreakMode.WordWrap;
						result.AddAttribute(UIStringAttributeKey.ParagraphStyle, wrapping, range);
					}
				});
			}

			return result;
		}

		static void FillMissingAttribute(NSAttributedString source, NSMutableAttributedString result, NSString key, NSObject fallback)
		{
			if (fallback is null)
			{
				return;
			}

			source.EnumerateAttribute(key, new NSRange(0, source.Length), NSAttributedStringEnumeration.None, (NSObject value, NSRange range, ref bool stop) =>
			{
				if (value is null)
				{
					result.AddAttribute(key, fallback, range);
				}
			});
		}

		bool IsSelectionTextViewCurrent(UITextView textView) =>
			textView.AttributedText.IsEqual(GetSelectionText()) &&
			textView.TextAlignment == TextAlignment &&
			textView.SemanticContentAttribute == SemanticContentAttribute &&
			textView.TextContainer.MaximumNumberOfLines == (nuint)Math.Max((int)Lines, 0) &&
			textView.TextContainer.LineBreakMode == LineBreakMode;

		void SyncSelectionTextView()
		{
			if (_selectionTextView is not { } textView)
			{
				return;
			}

			var rect = GetInsetRect(Bounds);
			var attributedText = GetSelectionText();

			// Assigning the text resets the selection, so only do it when the content changed.
			if (!textView.AttributedText.IsEqual(attributedText))
			{
				textView.AttributedText = attributedText;
			}

			textView.TextAlignment = TextAlignment;
			textView.SemanticContentAttribute = SemanticContentAttribute;
			textView.TextContainer.MaximumNumberOfLines = (nuint)Math.Max((int)Lines, 0);
			textView.TextContainer.LineBreakMode = LineBreakMode;

			var textHeight = nfloat.Min(textView.SizeThatFits(new SizeF(rect.Width, nfloat.MaxValue)).Height, rect.Height);
			var y = _verticalAlignment switch
			{
				UIControlContentVerticalAlignment.Top => rect.Y,
				UIControlContentVerticalAlignment.Bottom => rect.Bottom - textHeight,
				_ => rect.Y + (rect.Height - textHeight) / 2,
			};

			textView.Frame = new RectangleF(rect.X, y, rect.Width, textHeight);
		}

		RectangleF GetInsetRect(RectangleF rect)
		{
			var insets = TextInsets;

			// Respect RTL (flip left/right insets)
			if (EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
			{
				insets = new UIEdgeInsets(
					insets.Top,
					insets.Right,
					insets.Bottom,
					insets.Left);
			}

			return insets.InsetRect(rect);
		}

		public override void DrawText(RectangleF rect)
		{
			if (_selectionTextView is { } textView)
			{
				// The hosted text view draws the text. If something changed the label without a layout pass, resync.
				if (!IsSelectionTextViewCurrent(textView))
				{
					SetNeedsLayout();
				}

				return;
			}

			rect = GetInsetRect(rect);


			if (_verticalAlignment != UIControlContentVerticalAlignment.Center
				&& _verticalAlignment != UIControlContentVerticalAlignment.Fill)
			{
				rect = AlignVertical(rect);
			}

			base.DrawText(rect);
		}

		RectangleF AlignVertical(RectangleF rect)
		{
			var availableSize = rect.Size;
			var requiredHeight = Lines == 1 ? Font.LineHeight : base.SizeThatFits(rect.Size).Height;

			if (requiredHeight < availableSize.Height)
			{
				if (_verticalAlignment == UIControlContentVerticalAlignment.Top)
				{
					rect.Height = requiredHeight;
				}
				else if (_verticalAlignment == UIControlContentVerticalAlignment.Bottom)
				{
					rect = new RectangleF(rect.X, rect.Bottom - requiredHeight, rect.Width, requiredHeight);
				}
			}

			return rect;
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			// Prior to calculating the text size, reduce the padding, and then add the padding back in the AddInsets method.
			var adjustedWidth = size.Width - TextInsets.Left - TextInsets.Right;
			var adjustedHeight = size.Height - TextInsets.Top - TextInsets.Bottom;
			var requestedSize = base.SizeThatFits(new SizeF(adjustedWidth, adjustedHeight));

			// Let's be sure the label is not larger than the container
			return AddInsets(new Size
			{
				Width = nfloat.Min(requestedSize.Width, size.Width),
				Height = nfloat.Min(requestedSize.Height, size.Height),
			});
		}

		SizeF AddInsets(SizeF size) => new SizeF(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}
	}
}