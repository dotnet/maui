// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Models;
using Microsoft.Maui.Client.Providers.Android;

namespace Microsoft.Maui.Client.Services;

/// <summary>
/// Service for performing system health checks.
/// </summary>
public interface IDoctorService
{
	Task<DoctorReport> RunAllChecksAsync(CancellationToken cancellationToken = default);
	Task<DoctorReport> RunCategoryChecksAsync(string category, CancellationToken cancellationToken = default);
	Task<bool> TryFixAsync(FixInfo fix, CancellationToken cancellationToken = default);
}
