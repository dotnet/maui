using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiRadioButton : ContentView
	{
		IRadioButton _radioButton;
		UIAccessibilityTrait _accessibilityTraits;

		public MauiRadioButton(IRadioButton virtualView)
		{
			_radioButton = virtualView;
			CrossPlatformMeasure = virtualView.CrossPlatformMeasure;
			CrossPlatformArrange = virtualView.CrossPlatformArrange;
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

		public override string? AccessibilityValue
		{
			get => _radioButton.IsChecked ? "1" : "0";
			set { }
		}
	}
}