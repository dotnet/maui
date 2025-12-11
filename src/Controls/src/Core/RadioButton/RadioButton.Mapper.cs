#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="Type[@FullName='Microsoft.Maui.Controls.RadioButton']/Docs/*" />
	public partial class RadioButton
	{
		IMauiContext MauiContext => Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext not set");

		internal new static void RemapForControls()
		{
			RadioButtonHandler.Mapper.ReplaceMapping<RadioButton, IRadioButtonHandler>(nameof(IRadioButton.Content), MapContent);
#if ANDROID || WINDOWS
			//On iOS, since a custom approach is used for RadioButton, TextTransform is applied through the Label control.
			RadioButtonHandler.Mapper.ReplaceMapping<RadioButton, IRadioButtonHandler>(nameof(TextTransform), MapContent);
#endif
#if ANDROID
			RadioButtonHandler.PlatformViewFactory = CreatePlatformView;
#endif
		}
	}
}
