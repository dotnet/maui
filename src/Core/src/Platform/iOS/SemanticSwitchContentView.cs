using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform;

// This class should remain internal until further refactored.
// It is currently being used only for RadioButton accessibility on iOS.
internal class SemanticSwitchContentView : ContentView
{
	UIAccessibilityTrait _accessibilityTraits;

	internal SemanticSwitchContentView(IContentView virtualView)
	{
		CrossPlatformLayout = virtualView;
		IsAccessibilityElement = true;
		UserInteractionEnabled = true;
	 	AddGestureRecognizer(new UITapGestureRecognizer(OnTouched){
			CancelsTouchesInView = false,
			ShouldRecognizeSimultaneously = GetRecognizeSimultaneously
		});
	}

	private bool GetRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer) => true;

	private void OnTouched()
	{
		if(!IsFirstResponder)
		{
			BecomeFirstResponder();
			if(View != null)
				View.IsFocused = true;
		}
	}

	static UIAccessibilityTrait? s_switchAccessibilityTraits;

	static UIAccessibilityTrait SwitchAccessibilityTraits
	{
		get
		{
			if (s_switchAccessibilityTraits == null ||
				s_switchAccessibilityTraits == UIAccessibilityTrait.None)
			{
				s_switchAccessibilityTraits = new UISwitch().AccessibilityTraits;
			}

			return s_switchAccessibilityTraits ?? UIAccessibilityTrait.None;
		}
	}

	public override bool CanBecomeFirstResponder => true;
	public override bool CanResignFirstResponder => true;
	public override bool ResignFirstResponder()
	{
		if(View is not null)
		View.IsFocused = false;
		return base.ResignFirstResponder();
	}

	public override UIAccessibilityTrait AccessibilityTraits
	{
		get => _accessibilityTraits |= SwitchAccessibilityTraits;
		set => _accessibilityTraits = value | SwitchAccessibilityTraits;
	}
}