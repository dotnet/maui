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
		try
		{
			var options = new DeploymentInitializeOptions();
			Result = DeploymentManager.Initialize(options);
		}
		catch (Exception ex)
		{
			Result = new DeploymentResult(DeploymentStatus.Unknown, ex);
		}
	}

	public static DeploymentResult Result { get; private set; } = null!;

	public static void LogIfFailed(IServiceProvider services)
	{
		if (Result?.Status == DeploymentStatus.Ok)
			return;

		var logger = services.CreateLogger("Microsoft.Maui.DeploymentManagerAutoInitializer");
		if (logger is null)
			return;

		var error = Result?.ExtendedError;
		if (error is null)
		{
			logger.LogError($"Unknown WindowsAppRuntime.DeploymentManager.Initialize error.");
		}
		else
		{
			var hresult = string.Format("0x{0:X}", error.HResult);
			logger.LogError(error, "WindowsAppRuntime.DeploymentManager.Initialize error ({HResult}): {ErrorMessage}", hresult, error.Message);
		}
	}
}