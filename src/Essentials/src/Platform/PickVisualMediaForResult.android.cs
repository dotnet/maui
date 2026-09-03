using System;
using AndroidX.Collection;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel;

internal class PickVisualMediaForResult : ActivityForResultRequest<PickVisualMedia, AndroidUri>
{
	static readonly Lazy<PickVisualMediaForResult> LazyInstance = new(new PickVisualMediaForResult());

	public static PickVisualMediaForResult Instance => LazyInstance.Value;
}