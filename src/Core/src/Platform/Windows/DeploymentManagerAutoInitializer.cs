using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	internal static class DeploymentManagerAutoInitializer
	{
		[global::System.Runtime.CompilerServices.ModuleInitializer]
		internal static void AccessWindowsAppSDK()
		{
			Result = global::Microsoft.Windows.ApplicationModel.WindowsAppRuntime.DeploymentManager.Initialize(new());
		}

		public static global::Microsoft.Windows.ApplicationModel.WindowsAppRuntime.DeploymentResult Result { get; private set; }

		public static void ThrowIfFailed()
		{
			throw new Exception("yay");

			if (Result?.Status == DeploymentStatus.Ok)
				return;

			if (Result?.ExtendedError is null)
				throw new global::System.SystemException($"Unknown WindowsAppRuntime.DeploymentManager.Initialize error.");

			throw new global::System.SystemException($"WindowsAppRuntime.DeploymentManager.Initialize error (0x{Result.ExtendedError?.HResult:X}): {Result.ExtendedError?.Message}", Result.ExtendedError);
		}
	}
}