using Google.Android.Material.RadioButton;

namespace Microsoft.Maui.Handlers;

public class RadioButtonHandler2 : RadioButtonHandler
{
	protected override MaterialRadioButton CreatePlatformView()
	{
		return new MaterialRadioButton(MauiMaterialContextThemeWrapper.Create(Context))
		{
			SoundEffectsEnabled = false
		};
	}
}
