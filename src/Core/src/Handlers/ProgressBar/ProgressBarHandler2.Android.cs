using Google.Android.Material.ProgressIndicator;

namespace Microsoft.Maui.Handlers;

public class ProgressBarHandler2 : ProgressBarHandler
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
