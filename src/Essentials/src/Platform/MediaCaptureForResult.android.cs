using System.Threading.Tasks;
using AndroidX.Activity.Result.Contract;
using Microsoft.Maui.Media;
using AndroidUri = Android.Net.Uri;
using JavaBoolean = Java.Lang.Boolean;

namespace Microsoft.Maui.ApplicationModel;

/// <summary>
/// Handles AndroidX boolean media capture contracts and routes orphaned results into MediaPicker recovery.
/// </summary>
internal abstract class MediaCaptureForResult<TContract> : ActivityForResultRequest<TContract, JavaBoolean>
	where TContract : ActivityResultContract, new()
{
	readonly RecoveredMediaPickerResultKind resultKind;

	protected MediaCaptureForResult(RecoveredMediaPickerResultKind resultKind)
	{
		this.resultKind = resultKind;
	}

	public Task<JavaBoolean> Launch(AndroidUri input)
		=> base.Launch(input);

	protected override void OnActivityResultForActiveLaunch(JavaBoolean result)
		=> MediaPickerRecoveryManager.RecordCaptureCallbackResult(resultKind, result?.BooleanValue() == true);

	protected override void OnActivityResultForOrphanedLaunch(JavaBoolean result)
		=> MediaPickerRecoveryManager.RecoverOrphanedCaptureResult(resultKind, result?.BooleanValue() == true);
}
