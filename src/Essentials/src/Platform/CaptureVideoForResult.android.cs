using System;
using Microsoft.Maui.Media;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;

namespace Microsoft.Maui.ApplicationModel;

// Keep a separate singleton per AndroidX capture contract. Each contract needs its own
// registered launcher, while shared recovery behavior lives in MediaCaptureForResult<TContract>.
internal class CaptureVideoForResult : MediaCaptureForResult<CaptureVideo>
{
	static readonly Lazy<CaptureVideoForResult> LazyInstance = new(new CaptureVideoForResult());

	public static CaptureVideoForResult Instance => LazyInstance.Value;

	CaptureVideoForResult()
		: base(RecoveredMediaPickerResultKind.CaptureVideo)
	{
	}
}
