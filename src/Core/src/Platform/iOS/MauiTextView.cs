﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiTextView : UITextView, IUIViewLifeCycleEvents
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		readonly MauiLabel _placeholderLabel;
		nfloat? _defaultPlaceholderSize;

		public MauiTextView()
		{
			_placeholderLabel = InitPlaceholderLabel();
			UpdatePlaceholderLabelFrame();
			Changed += OnChanged;
		}

		public MauiTextView(CGRect frame)
			: base(frame)
		{
			_placeholderLabel = InitPlaceholderLabel();
			UpdatePlaceholderLabelFrame();
			Changed += OnChanged;
		}

		public override void WillMoveToWindow(UIWindow? window)
		{
			base.WillMoveToWindow(window);
		}

		// Native Changed doesn't fire when the Text Property is set in code
		// We use this event as a way to fire changes whenever the Text changes
		// via code or user interaction.
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public event EventHandler? TextSetOrChanged;

		public string? PlaceholderText
		{
			get => _placeholderLabel.Text;
			set
			{
				_placeholderLabel.Text = value;
				_placeholderLabel.SizeToFit();
			}
		}

		public NSAttributedString? AttributedPlaceholderText
		{
			get => _placeholderLabel.AttributedText;
			set
			{
				_placeholderLabel.AttributedText = value;
				_placeholderLabel.SizeToFit();
			}
		}

		public UIColor? PlaceholderTextColor
		{
			get => _placeholderLabel.TextColor;
			set => _placeholderLabel.TextColor = value;
		}

		public TextAlignment VerticalTextAlignment { get; set; }

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
				{
					HidePlaceholderIfTextIsPresent(value);
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public override UIFont? Font
		{
			get => base.Font;
			set
			{
				base.Font = value;
				UpdatePlaceholderFont(value);

			}
		}

		public override NSAttributedString AttributedText
		{
			get => base.AttributedText;
			set
			{
				var old = base.AttributedText;

				base.AttributedText = value;

				if (old?.Value != value?.Value)
				{
					HidePlaceholderIfTextIsPresent(value?.Value);
					TextSetOrChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			UpdatePlaceholderLabelFrame();
			AlignTextVertically();
		}

		MauiLabel InitPlaceholderLabel()
		{
			var placeholderLabel = new MauiLabel
			{
				BackgroundColor = UIColor.Clear,
				TextColor = ColorExtensions.PlaceholderColor,
				Lines = 0,
				VerticalAlignment = UIControlContentVerticalAlignment.Top
			};

			AddSubview(placeholderLabel);

			return placeholderLabel;
		}

		void UpdatePlaceholderLabelFrame()
		{
			if (Bounds != CGRect.Empty && _placeholderLabel is not null)
			{
				var x = TextContainer.LineFragmentPadding;
				var y = TextContainerInset.Top;
				var width = Bounds.Width - (x * 2);
				var height = Frame.Height - (TextContainerInset.Top + TextContainerInset.Bottom);

				_placeholderLabel.Frame = new CGRect(x, y, width, height);
			}
		}

	 	void AlignTextVertically()
		{
			var height = Bounds.Size.Height;
			var contentHeight = ContentSize.Height;
			nfloat topOffset;

			switch (VerticalTextAlignment)
			{
				case Maui.TextAlignment.Start:
					ContentOffset = CGPoint.Empty;
					break;

				case Maui.TextAlignment.Center:
					topOffset = (height - contentHeight * ZoomScale) / 2.0f;
					topOffset = topOffset < 0 ? 0 : topOffset;
					ContentOffset = new CGPoint(0, -topOffset);
					break;

				case Maui.TextAlignment.End:
					topOffset = height - contentHeight;
					topOffset = topOffset < 0 ? 0 : topOffset;
					ContentOffset = new CGPoint(0, -topOffset);
					break;
			}

			if (contentHeight >= height)
			{
				topOffset = contentHeight - height;
				topOffset = topOffset < 0 ? 0 : topOffset;
				ContentOffset = new CGPoint(0, topOffset);
			}
		}

		void HidePlaceholderIfTextIsPresent(string? value)
		{
			_placeholderLabel.Hidden = !string.IsNullOrEmpty(value);
		}

		void OnChanged(object? sender, EventArgs e)
		{
			HidePlaceholderIfTextIsPresent(Text);
			TextSetOrChanged?.Invoke(this, EventArgs.Empty);
		}

		void UpdatePlaceholderFont(UIFont? value)
		{
			_defaultPlaceholderSize ??= _placeholderLabel.Font.PointSize;
			_placeholderLabel.Font = value ?? _placeholderLabel.Font.WithSize(
				value?.PointSize ?? _defaultPlaceholderSize.Value);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
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
