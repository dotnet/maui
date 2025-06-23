using System;
using Android.Runtime;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;

namespace Microsoft.Maui.ApplicationModel;

internal class PickMultipleVisualMediaForResult : ActivityForResultRequest<PickMultipleVisualMedia, JavaList>
{
	static readonly Lazy<PickMultipleVisualMediaForResult> LazyInstance = new(new PickMultipleVisualMediaForResult());

	public static PickMultipleVisualMediaForResult Instance => LazyInstance.Value;
}