using System;
using System.Diagnostics.CodeAnalysis;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform
{
	public class NoCaretField : UITextField, IUIViewLifeCycleEvents
	{
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

		// On iOS/macOS 26+, UITextField with BorderStyle = RoundedRect renders with a
		// pill/capsule-shaped focus ring due to the new "Liquid Glass" design language.
		// We set FocusEffect in LayoutSubviews (not via property getter) so that Bounds
		// is guaranteed to be non-zero when the path is created.
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if ((OperatingSystem.IsMacCatalystVersionAtLeast(26) || OperatingSystem.IsIOSVersionAtLeast(26))
				&& Bounds.Width > 0 && Bounds.Height > 0)
			{
				base.FocusEffect = UIFocusHaloEffect.Create(UIBezierPath.FromRoundedRect(Bounds, 8.0f));
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