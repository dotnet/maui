using System;
using Microsoft.Maui.Media;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;

namespace Microsoft.Maui.ApplicationModel;

// Keep a separate singleton per AndroidX capture contract. Each contract needs its own
// registered launcher, while shared recovery behavior lives in MediaCaptureForResult<TContract>.
internal class CapturePhotoForResult : MediaCaptureForResult<TakePicture>
{
	static readonly Lazy<CapturePhotoForResult> LazyInstance = new(() => new CapturePhotoForResult());

	public static CapturePhotoForResult Instance => LazyInstance.Value;

	CapturePhotoForResult()
		: base(RecoveredMediaPickerResultKind.CapturePhoto)
	{
	}
}
