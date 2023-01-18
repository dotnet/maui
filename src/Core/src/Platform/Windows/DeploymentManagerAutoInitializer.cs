using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.ApplicationModel.WindowsAppRuntime;

namespace Microsoft.Maui;

internal static class DeploymentManagerAutoInitializer
{
#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
	[ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static void AccessWindowsAppSDK()
	{
		//var options = new DeploymentInitializeOptions();
		//Result = DeploymentManager.Initialize(options);
	}

	public static DeploymentResult Result { get; private set; } = null!;

	public static void ThrowIfFailed()
	{
		if (Result?.Status == DeploymentStatus.Ok)
			return;

		if (Result?.ExtendedError is null)
			throw new SystemException($"Unknown WindowsAppRuntime.DeploymentManager.Initialize error.");

		throw new SystemException($"WindowsAppRuntime.DeploymentManager.Initialize error (0x{Result.ExtendedError?.HResult:X}): {Result.ExtendedError?.Message}", Result.ExtendedError);
	}

	public static void LogIfFailed(IServiceProvider services)
	{
		if (Result?.Status == DeploymentStatus.Ok)
			return;

		var logger = services.CreateLogger("Microsoft.Maui.DeploymentManagerAutoInitializer");
		if (logger is null)
			return;

		if (Result?.ExtendedError is null)
			logger.LogError($"Unknown WindowsAppRuntime.DeploymentManager.Initialize error.");
		else
			logger.LogError(Result.ExtendedError, "WindowsAppRuntime.DeploymentManager.Initialize error ({HResult}): {ErrorMessage}", Result.ExtendedError?.HResult, Result.ExtendedError?.Message);
	}
}