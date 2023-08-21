// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Android.App;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, Application>
	{
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
			var currentActivity = ApplicationModel.Platform.CurrentActivity;

			if (currentActivity != null)
			{
				currentActivity.FinishAndRemoveTask();

				Environment.Exit(0);
			}
		}

		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView?.RequestNewWindow(application, args as OpenWindowRequest);
		}

		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				if (window.Handler?.PlatformView is Activity activity)
					activity.Finish();
			}
		}
	}
}