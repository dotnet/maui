// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using AudioToolbox;

namespace Microsoft.Maui.Devices
{
	partial class VibrationImplementation : IVibration
	{
		public bool IsSupported => true;

		void PlatformVibrate() =>
			SystemSound.Vibrate.PlaySystemSound();

		void PlatformVibrate(TimeSpan duration) =>
			SystemSound.Vibrate.PlaySystemSound();

		void PlatformCancel()
		{
		}
	}
}
