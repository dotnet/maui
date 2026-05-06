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
		// Override FocusEffect to return a UIFocusHaloEffect with a traditional 8pt
		// rounded rect shape so the focus ring matches the visible border shape.
		public override UIFocusEffect? FocusEffect
		{
			get
			{
				if (OperatingSystem.IsMacCatalystVersionAtLeast(26) || OperatingSystem.IsIOSVersionAtLeast(26))
					return UIFocusHaloEffect.Create(UIBezierPath.FromRoundedRect(Bounds, 8.0f));
				return base.FocusEffect;
			}
			set => base.FocusEffect = value;
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