// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.ApplicationModel
{
	class AppInfoImplementation : IAppInfo
	{
		public string PackageName => throw ExceptionUtils.NotSupportedOrImplementedException;

		public string Name => throw ExceptionUtils.NotSupportedOrImplementedException;

		public System.Version Version => Utils.ParseVersion(VersionString);

		public string VersionString => throw ExceptionUtils.NotSupportedOrImplementedException;

		public string BuildString => throw ExceptionUtils.NotSupportedOrImplementedException;

		public void ShowSettingsUI() => throw ExceptionUtils.NotSupportedOrImplementedException;

		public AppTheme RequestedTheme => AppTheme.Unspecified;

		public AppPackagingModel PackagingModel => throw ExceptionUtils.NotSupportedOrImplementedException;

		// Returning the Unknown value for LayoutDirection so that unit tests can work
		public LayoutDirection RequestedLayoutDirection => LayoutDirection.Unknown;
	}
}
