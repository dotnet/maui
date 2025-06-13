using System;
using Android.Util;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;

namespace Microsoft.Maui.ApplicationModel;

internal class PickMultipleVisualMediaForResult : ActivityForResultRequest<PickMultipleVisualMedia, ArraySet>
{
	static readonly Lazy<PickMultipleVisualMediaForResult> LazyInstance = new(new PickMultipleVisualMediaForResult());

	public static PickMultipleVisualMediaForResult Instance => LazyInstance.Value;
}