using System;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{
		IMauiContext MauiContext => Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext not set");

		public static IPropertyMapper<RadioButton, RadioButtonHandler> ControlsRadioButtonMapper =
			   new PropertyMapper<RadioButton, RadioButtonHandler>(RadioButtonHandler.Mapper)
			   {
#if IOS || ANDROID
				   [nameof(IRadioButton.Content)] = MapContent
#endif
			   };

		internal new static void RemapForControls()
		{
			RadioButtonHandler.Mapper = ControlsRadioButtonMapper;

#if ANDROID
			RadioButtonHandler.NativeViewFactory = CreatePlatformView;
#endif
		}
	}
}
