using Google.Android.Material.ProgressIndicator;

namespace Microsoft.Maui.Handlers;

// TODO: Material3 - make it public in .net 11
internal class ProgressBarHandler2 : ProgressBarHandler
{
	protected override LinearProgressIndicator CreatePlatformView()
	{
		return new LinearProgressIndicator(MauiMaterialContextThemeWrapper.Create(Context))
		{
			Indeterminate = false,
			Max = ProgressBarExtensions.Maximum
		};
	}
}