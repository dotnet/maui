using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{

	public class MauiTextField : UITextField, IUIViewLifeCycleEvents
	{
		// UITextField's RoundedRect border uses an approximately 5pt corner radius.
		const float roundedRectCornerRadius = 5f;

		CGSize _lastFocusHaloBoundsSize;
		UITextBorderStyle _lastFocusHaloBorderStyle;

		public MauiTextField(CGRect frame)
			: base(frame)
		{
		}

		public MauiTextField()
		{
		}

		public override void WillMoveToWindow(UIWindow? window)
		{
			base.WillMoveToWindow(window);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			// On MacCatalyst 26+, the system's default keyboard focus halo around a
			// UITextField with BorderStyle == RoundedRect renders as a fully rounded
			// (pill-shaped) outline that doesn't match the field's actual corner radius.
			// Provide a custom UIFocusHaloEffect whose path follows the field's
			// rounded-rect bounds so the halo aligns with the border.
			// See: https://developer.apple.com/documentation/uikit/uifocushaloeffect
			if (OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				if (BorderStyle == UITextBorderStyle.RoundedRect
				 && Bounds.Width > 0 && Bounds.Height > 0
				 && (Bounds.Size != _lastFocusHaloBoundsSize || BorderStyle != _lastFocusHaloBorderStyle))
				{
					_lastFocusHaloBoundsSize = Bounds.Size;
					_lastFocusHaloBorderStyle = BorderStyle;
					var path = UIBezierPath.FromRoundedRect(Bounds, roundedRectCornerRadius);
					FocusEffect = UIFocusHaloEffect.Create(path);
				}
				else if (BorderStyle != UITextBorderStyle.RoundedRect && _lastFocusHaloBorderStyle == UITextBorderStyle.RoundedRect)
				{
					_lastFocusHaloBorderStyle = BorderStyle;
					FocusEffect = null;
				}
			}
		}

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value && !_suppressTextPropertySet)
					TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public override NSAttributedString? AttributedText
		{
			get => base.AttributedText;
			set
			{
				var old = base.AttributedText;

				base.AttributedText = value;

				if (old?.Value != value?.Value)
					TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public override UITextRange? SelectedTextRange
		{
			get => base.SelectedTextRange;
			set
			{
				var old = base.SelectedTextRange;

				base.SelectedTextRange = value;

				if (old?.Start != value?.Start || old?.End != value?.End)
					SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
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

		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public event EventHandler? TextPropertySet;
		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		internal event EventHandler? SelectionChanged;

		/// <summary>
		/// When set to <c>true</c>, suppresses the <see cref="TextPropertySet"/> event during
		/// controlled text updates (e.g., preserving text across IsPassword toggles) to avoid
		/// triggering unintended binding/handler side effects.
		/// </summary>
		bool _suppressTextPropertySet;

		internal void SuppressTextPropertySet(bool suppress)
		{
			_suppressTextPropertySet = suppress;
		}
	}
}