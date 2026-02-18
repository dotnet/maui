#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A mutually exclusive selection control that allows users to select one option from a set.
	/// </summary>
	public partial class RadioButton
	{
		IMauiContext MauiContext => Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext not set");

		static RadioButton()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(RadioButton), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			RadioButtonHandler.Mapper.ReplaceMapping<RadioButton, IRadioButtonHandler>(nameof(IRadioButton.Content), MapContent);

#if ANDROID
			RadioButtonHandler.PlatformViewFactory = CreatePlatformView;
#endif
		}
	}
}
