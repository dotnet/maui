using System;
using Microsoft.Maui.Media;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel;

internal class PickVisualMediaForResult : ActivityForResultRequest<PickVisualMedia, AndroidUri>
{
	static readonly Lazy<PickVisualMediaForResult> LazyInstance = new(new PickVisualMediaForResult());

	public static PickVisualMediaForResult Instance => LazyInstance.Value;

	protected override void OnActivityResultForActiveLaunch(AndroidUri result)
		=> MediaPickerRecoveryManager.RecordSinglePickCallbackResult(result, materializeImmediately: true);

	protected override void OnActivityResultForOrphanedLaunch(AndroidUri result)
		=> MediaPickerRecoveryManager.RecoverOrphanedSinglePickResult(result);
}
