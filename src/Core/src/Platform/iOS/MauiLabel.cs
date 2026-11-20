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
				}

				SetNeedsDisplay();
			}
		}

		internal bool IsSelectionView(UIView view) =>
			_selectionTextView is not null && view is not null && view.IsDescendantOfView(_selectionTextView);

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

		bool IsSelectionTextViewCurrent(UITextView textView) =>
			textView.AttributedText.IsEqual(AttributedText) &&
			textView.TextContainer.MaximumNumberOfLines == (nuint)Math.Max((int)Lines, 0) &&
			textView.TextContainer.LineBreakMode == LineBreakMode;

		void SyncSelectionTextView()
		{
			if (_selectionTextView is not { } textView)
			{
				return;
			}

			var rect = GetInsetRect(Bounds);
			var attributedText = AttributedText ?? new NSAttributedString();

			// Assigning the text resets the selection, so only do it when the content changed.
			if (!textView.AttributedText.IsEqual(attributedText))
			{
				textView.AttributedText = attributedText;
			}

			textView.TextAlignment = TextAlignment;
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