// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.DevTools.Errors;

/// <summary>
/// Error code taxonomy for MAUI Dev Tools.
/// Format: E{category}{number}
/// Categories:
///   1xxx - Tool errors (internal bugs)
///   2xxx - Platform/SDK errors
///   3xxx - User action required
///   4xxx - Network errors
///   5xxx - Permission errors
/// </summary>
public static class ErrorCodes
{
	// Tool errors (E1xxx)
	public const string InternalError = "E1001";
	public const string InvalidConfiguration = "E1002";
	public const string CommandNotSupported = "E1003";
	public const string InvalidArgument = "E1004";
	public const string NotImplemented = "E1005";
	public const string DeviceNotFound = "E1006";
	public const string PlatformNotSupported = "E1007";

	// Platform/SDK errors - JDK (E20xx)
	public const string JdkNotFound = "E2001";
	public const string JdkVersionUnsupported = "E2002";
	public const string JdkInstallFailed = "E2003";

	// Platform/SDK errors - Android SDK (E21xx)
	public const string AndroidSdkNotFound = "E2101";
	public const string AndroidSdkManagerNotFound = "E2102";
	public const string AndroidLicensesNotAccepted = "E2103";
	public const string AndroidPackageNotFound = "E2104";
	public const string AndroidPackageInstallFailed = "E2105";
	public const string AndroidEmulatorNotFound = "E2106";
	public const string AndroidAvdNotFound = "E2107";
	public const string AndroidAvdCreateFailed = "E2108";
	public const string AndroidAvdStartFailed = "E2109";
	public const string AndroidAdbNotFound = "E2110";
	public const string AndroidDeviceNotFound = "E2111";
	public const string AndroidAvdDeleteFailed = "E2112";

	// Platform/SDK errors - Apple/Xcode (E22xx)
	public const string XcodeNotFound = "E2201";
	public const string XcodeCliToolsNotFound = "E2202";
	public const string XcodeCommandLineToolsNotFound = "E2202"; // Alias
	public const string SimctlFailed = "E2203";
	public const string AppleSimctlFailed = "E2203"; // Alias
	public const string SimulatorNotFound = "E2204";
	public const string SimulatorBootFailed = "E2205";
	public const string RuntimeNotFound = "E2206";
	public const string RuntimeInstallFailed = "E2207";
	public const string XcodeSelectFailed = "E2208";
	public const string AppleNoSimulators = "E2209";

	// Platform/SDK errors - Windows (E23xx)
	public const string WindowsSdkNotFound = "E2301";
	public const string DeveloperModeNotEnabled = "E2302";

	// Platform/SDK errors - .NET (E24xx)
	public const string DotNetNotFound = "E2401";
	public const string DotNetSdkNotFound = "E2401"; // Alias for compatibility
	public const string MauiWorkloadMissing = "E2402";
	public const string MauiWorkloadNotInstalled = "E2402"; // Alias for compatibility

	// User action required (E3xxx)
	public const string XcodeInstallRequired = "E3001";
	public const string DeveloperModeEnableRequired = "E3002";
	public const string InsufficientDiskSpace = "E3003";
	public const string ManualStepsRequired = "E3004";
	public const string LicenseAcceptanceRequired = "E3005";

	// Network errors (E4xxx)
	public const string NetworkUnavailable = "E4001";
	public const string DownloadFailed = "E4002";
	public const string TimeoutExpired = "E4003";

	// Permission errors (E5xxx)
	public const string ElevationRequired = "E5001";
	public const string AccessDenied = "E5002";
	public const string AgentPermissionDenied = "E5003";
}
