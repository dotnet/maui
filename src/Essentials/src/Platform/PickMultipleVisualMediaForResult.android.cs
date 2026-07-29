using System;
using System.Collections.Generic;
using Android.Runtime;
using Microsoft.Maui.Media;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel;

internal class PickMultipleVisualMediaForResult : ActivityForResultRequest<PickMultipleVisualMedia, JavaList>
{
	static readonly Lazy<PickMultipleVisualMediaForResult> LazyInstance = new(() => new PickMultipleVisualMediaForResult());

	public static PickMultipleVisualMediaForResult Instance => LazyInstance.Value;

	protected override void OnActivityResultForActiveLaunch(JavaList result)
		=> MediaPickerRecoveryManager.RecordMultiplePickCallbackResult(ToAndroidUris(result));

	protected override void OnActivityResultForOrphanedLaunch(JavaList result)
		=> MediaPickerRecoveryManager.RecoverOrphanedMultiplePickResult(ToAndroidUris(result));

	static IReadOnlyList<AndroidUri> ToAndroidUris(JavaList result)
	{
		if (result is null || result.IsEmpty)
		{
			return Array.Empty<AndroidUri>();
		}

		var uris = new List<AndroidUri>();
		for (var i = 0; i < result.Size(); i++)
		{
			if (result.Get(i) is AndroidUri uri)
			{
				uris.Add(uri);
			}
		}

		return uris;
	}
}
