// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		public static void MapWindowSoftInputModeAdjust(ApplicationHandler handler, Application application)
		{
			Platform.ApplicationExtensions.UpdateWindowSoftInputModeAdjust(handler.PlatformView, application);
		}
	}
}