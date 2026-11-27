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

		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			RadioButtonHandler.Mapper.ReplaceMappingForControls<RadioButton, IRadioButtonHandler>(nameof(IRadioButton.Content), MapContent);
#if ANDROID || WINDOWS
			//On iOS, since a custom approach is used for RadioButton, TextTransform is applied through the Label control.
			RadioButtonHandler.Mapper.ReplaceMappingForControls<RadioButton, IRadioButtonHandler>(nameof(TextTransform), MapContent);
#endif
#if ANDROID
			// This runs lazily on first attachment; keep a factory the app installed during startup, which would
			// have replaced the Controls factory when it was installed eagerly.
			RadioButtonHandler.PlatformViewFactory ??= CreatePlatformView;
#endif
		}
	}
}
