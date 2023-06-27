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
	}

	static UIKit.UIAccessibilityTrait? s_switchAccessibilityTraits;
	UIKit.UIAccessibilityTrait SwitchAccessibilityTraits
	{
		get
		{
			if (s_switchAccessibilityTraits == null ||
				s_switchAccessibilityTraits == UIKit.UIAccessibilityTrait.None)
			{
				s_switchAccessibilityTraits = new UIKit.UISwitch().AccessibilityTraits;
			}

			return s_switchAccessibilityTraits ?? UIKit.UIAccessibilityTrait.None;
		}
	}

	public override UIAccessibilityTrait AccessibilityTraits
	{
		get => _accessibilityTraits |= SwitchAccessibilityTraits;
		set => _accessibilityTraits = value | SwitchAccessibilityTraits;
	}
}