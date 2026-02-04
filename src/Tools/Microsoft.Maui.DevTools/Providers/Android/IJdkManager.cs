// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;

namespace Microsoft.Maui.DevTools.Providers.Android;

/// <summary>
/// Interface for JDK management operations.
/// </summary>
public interface IJdkManager
{
	/// <summary>
	/// Gets the detected JDK path.
	/// </summary>
	string? DetectedJdkPath { get; }

	/// <summary>
	/// Gets the detected JDK version.
	/// </summary>
	int? DetectedJdkVersion { get; }

	/// <summary>
	/// Gets whether JDK is installed.
	/// </summary>
	bool IsInstalled { get; }

	/// <summary>
	/// Performs health check for JDK.
	/// </summary>
	Task<HealthCheck> CheckHealthAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Installs JDK.
	/// </summary>
	Task InstallAsync(int version = 17, string? installPath = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets available JDK versions for installation.
	/// </summary>
	IEnumerable<int> GetAvailableVersions();
}
