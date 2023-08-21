// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class ProgressBarExtensions
	{
		public static void UpdateProgress(this UIProgressView platformProgressBar, IProgress progress)
		{
			platformProgressBar.Progress = (float)progress.Progress;
		}

		public static void UpdateProgressColor(this UIProgressView platformProgressBar, IProgress progress)
		{
			platformProgressBar.ProgressTintColor = progress.ProgressColor?.ToPlatform();
		}
	}
}