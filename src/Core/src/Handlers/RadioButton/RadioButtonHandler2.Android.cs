using Google.Android.Material.RadioButton;

namespace Microsoft.Maui.Handlers;

// TODO: Material3 - make it public in .net 11
internal class RadioButtonHandler2 : RadioButtonHandler
{
	protected override MaterialRadioButton CreatePlatformView()
	{
		return new MaterialRadioButton(MauiMaterialContextThemeWrapper.Create(Context))
		{
			SoundEffectsEnabled = false
		};
	}
}