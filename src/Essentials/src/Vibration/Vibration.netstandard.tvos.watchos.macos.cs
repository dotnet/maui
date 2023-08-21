// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	partial class VibrationImplementation : IVibration
	{
		public bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformVibrate()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformVibrate(TimeSpan duration)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformCancel()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
