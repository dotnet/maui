using System;
using System.Diagnostics.CodeAnalysis;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform
{
	public class NoCaretField : UITextField, IUIViewLifeCycleEvents
	{
		// UITextField's RoundedRect border uses an approximately 5pt corner radius.
		const float roundedRectCornerRadius = 5f;

		CoreGraphics.CGSize _lastFocusHaloBoundsSize;
		UITextBorderStyle _lastFocusHaloBorderStyle;

		public NoCaretField() : base(new RectangleF())
		{
			SpellCheckingType = UITextSpellCheckingType.No;
			AutocorrectionType = UITextAutocorrectionType.No;
			AutocapitalizationType = UITextAutocapitalizationType.None;
		}

		public override RectangleF GetCaretRectForPosition(UITextPosition? position)
		{
			return RectangleF.Empty;
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

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
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