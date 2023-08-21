// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform
{
	public static class ActivityIndicatorExtensions
	{
		public static void UpdateIsRunning(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			progressBar.Visibility = GetActivityIndicatorVisibility(activityIndicator);
		}

		internal static ViewStates GetActivityIndicatorVisibility(this IActivityIndicator activityIndicator)
		{
			if (activityIndicator.Visibility == Visibility.Visible)
			{
				return activityIndicator.IsRunning ? ViewStates.Visible : ViewStates.Invisible;
			}
			else
			{
				return activityIndicator.Visibility.ToPlatformVisibility();
			}
		}

		public static void UpdateColor(this ProgressBar progressBar, IActivityIndicator activityIndicator)
		{
			var color = activityIndicator.Color;

			if (color != null)
				progressBar.IndeterminateDrawable?.SetColorFilter(color.ToPlatform(), FilterMode.SrcIn);
			else
				progressBar.IndeterminateDrawable?.ClearColorFilter();
		}
	}
}