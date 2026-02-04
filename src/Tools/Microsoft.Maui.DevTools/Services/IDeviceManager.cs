// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.DevTools.Models;

namespace Microsoft.Maui.DevTools.Services;

/// <summary>
/// Service for managing devices across all platforms.
/// </summary>
public interface IDeviceManager
{
	Task<IReadOnlyList<Device>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
	Task<IReadOnlyList<Device>> GetDevicesByPlatformAsync(string platform, CancellationToken cancellationToken = default);
	Task<Device?> GetDeviceByIdAsync(string deviceId, CancellationToken cancellationToken = default);
	Task<string> TakeScreenshotAsync(string deviceId, string? outputPath = null, CancellationToken cancellationToken = default);
}
