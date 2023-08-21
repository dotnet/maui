// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ProgressBarStub : StubBase, IProgress
	{
		public double Progress { get; set; }

		public Color ProgressColor { get; set; }
	}
}