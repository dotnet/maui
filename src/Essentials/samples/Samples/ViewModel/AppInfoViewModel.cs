// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Samples.ViewModel
{
	public class AppInfoViewModel : BaseViewModel
	{
		public string AppPackageName => AppInfo.PackageName;

		public string AppName => AppInfo.Name;

		public string AppVersion => AppInfo.VersionString;

		public string AppBuild => AppInfo.BuildString;

		public string AppTheme => AppInfo.RequestedTheme.ToString();

		public Command ShowSettingsUICommand { get; }

		public AppInfoViewModel()
		{
			ShowSettingsUICommand = new Command(() => AppInfo.ShowSettingsUI());
		}
	}
}
